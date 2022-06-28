import { Injectable } from '@angular/core';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { Observable, BehaviorSubject } from 'rxjs';
import { Action } from './action';

export enum ActionState {
  Locked,
  Unlocked
}

@Injectable({
  providedIn: 'root'
})
export class ActionsStateService {
  constructor(private logger: LoggerService) {}
  private _pendingActions = new Map<asiaGuid, { obs$: BehaviorSubject<ActionState>; actions: Action[] }>();

  add(id: asiaGuid, action: Action) {
    if (!this._pendingActions.has(id))
      this._pendingActions.set(id, {
        obs$: new BehaviorSubject<ActionState>(ActionState.Locked),
        actions: []
      });

    this._pendingActions.get(id)?.actions.push(action);
    this._pendingActions.get(id)?.obs$.next(ActionState.Locked);

    action.loading$.subscribe({
      complete: () => {
        const pendingActions = this._pendingActions.get(id)!;
        pendingActions.actions.splice(pendingActions.actions.indexOf(action), 1);
        if (pendingActions.actions.length === 0) pendingActions.obs$.next(ActionState.Unlocked);
      }
    });
  }

  getState(id: asiaGuid): Observable<ActionState> {
    if (id === undefined) this.logger.logWarn('[ActionsStateService] id passed into getState is undefined');
    const actionState = this._pendingActions.get(id);
    if (actionState === undefined) {
      return this._pendingActions
        .set(id, { obs$: new BehaviorSubject<ActionState>(ActionState.Unlocked), actions: [] })
        .get(id)!
        .obs$.asObservable();
    }
    return actionState.obs$.asObservable();
  }
}
