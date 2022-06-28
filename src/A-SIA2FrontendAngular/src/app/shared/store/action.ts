import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { Guid } from 'guid-typescript';
import { Subject } from 'rxjs';
import { ActionsStateService } from './actions-state.service';

// a basic store action, which can act, and revert in case of error.
// creating a class for each action allows for more flexible caching, or undoing through the command pattern.
// the revert function may also be called inside of act()
// implement this to define a store action, like delete, update, insert, etc.
export abstract class Action {
  constructor(protected actionsState: ActionsStateService, protected logger: LoggerService) {}

  abstract act(): void;
  abstract revert(): void;
  protected _loading$ = new Subject<void>();
  public readonly loading$ = this._loading$.asObservable();

  completeLoading(): void {
    this._loading$.next();
    this._loading$.complete();
  }

  failLoading(message: string, error: string): void {
    this.logger.logError(message, error);
    this._loading$.error(error);
    this._loading$.complete();
  }

  failLoadingAndRevert(message: string, error: string): void {
    this.failLoading(message, error);
    this.revert();
  }
}
