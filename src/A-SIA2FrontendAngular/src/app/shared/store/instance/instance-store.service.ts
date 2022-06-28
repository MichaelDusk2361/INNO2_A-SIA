import { Injectable } from '@angular/core';
import { InstanceService } from '@shared/backend/instance.service';
import { UserService } from '@shared/backend/user.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaInstance } from '@shared/models/instance.model';
import { DetailsNode } from '@shared/models/other/DetailsNode';
import { ignoreErrors } from '@shared/operators/ignoreErrors';
import { BehaviorSubject, Observable } from 'rxjs';
import { Action } from '../action';
import { ActionsStateService } from '../actions-state.service';
import { ProjectHierarchyStoreService } from '../project-hierarchy/project-hierarchy-store.service';
import { InstanceDeleteAction } from './actions/delete';
import { InstanceInsertAction } from './actions/insert';
import { InstanceUpdateAction } from './actions/update';

/**
 * An example implementation of a store, using rxjs, it is basically a primitive implementation of the redux pattern / ngrx.
 * This store especially, implements endpoints for optimistic UI updates, which changes the UI (aka. cached values), sends
 * the server request afterwards, and reverts the UI in case of an error.
 *
 * The store uses BehaviorSubjects to cache, and publish values (in this case asiaInstances).
 * BehaviorSubjects are especially advantageous as they always have a value, and therefore don't necessarily need to be subscribed to.
 * Instead, a value can be retrieved by calling .getValue (or _getSourceValues in the store). The BehaviorSubject is exposed to the outside as
 * an Observable (get values$()), so that no-one can do responsibility breaking stuff with the subject itself (like calling .next() in a component).
 *
 * The $ symbol at the end of variable / getter names is a naming convention commonly used for observables.
 *
 * Every store has a initialization (_load()), a way to get cached values from the inside (_getSourceValues) / outside (values$) and
 * a way to set values from the inside (_setSourceValues()).
 *
 * Be careful to keep values immutable, that is, don't change an existing cached value. Rather, copy all values, add / remove some, and then _setSourceValues();.
 * This can be easily done with the spread operator. This ensures that references change when notifying subscribers, and therefore enables ChangeDetection.OnPush
 * for future performance gains.
 *
 * In some cases, especially when wanting to enable undo / redo, it might be advantageous to extract actions like add, update, and
 * delete, into their own classes {@link Action}. While not only keeping the Store classes themselves slim, as they only have to
 * call .act(), and maybe .revert(), this also makes caching possibly to be reverted values easier.
 * Actions usually also have a loading$ observable that indicate wether they are done / successful.
 *
 */
@Injectable({
  providedIn: 'root'
})
export class InstanceStoreService {
  readonly _source = new BehaviorSubject<asiaInstance[]>([]);
  get values$(): Observable<asiaInstance[]> {
    return this._source.asObservable().pipe(this.logger.rxjsDebug('[InstanceStoreService] get values$'));
  }

  constructor(
    private userService: UserService,
    private instanceService: InstanceService,
    private hierarchy: ProjectHierarchyStoreService,
    private logger: LoggerService,
    private actionState: ActionsStateService
  ) {
    this._load();
  }
  _load(): void {
    this.userService.getInstances$.subscribe((res) => this._source.next(res));
  }
  _getSourceValues(): asiaInstance[] {
    return this._source.getValue();
  }
  _setSourceValues(instances: asiaInstance[]): void {
    this._source.next([...instances]); // the objects are copied again just in case, probably unnecessary.
  }

  /**
   * @param instance instance to be added, will be defaulted to if not provided
   * @returns loading status of the actual request
   */
  add(instance?: asiaInstance): Action {
    if (instance === undefined) instance = new asiaInstance('Unnamed Instance', '');
    const insertAction = new InstanceInsertAction(this, instance, this.instanceService, this.actionState, this.logger);
    const hierarchyInsertAction = this.hierarchy.addInstance(instance); // add to hierarchy as well
    insertAction.act(); // execute the action, updates frontend ui already
    // do the actual post request
    insertAction.loading$.subscribe({
      error: (err) => {
        this.logger.logError('[InstanceStoreService] add', err);
        hierarchyInsertAction?.revert();
      }
    });
    return insertAction;
  }

  /**
   *
   * @param instance instance to update, watch out that the id matches the backend
   * @returns loading status of the actual request
   */
  update(instance: asiaInstance): Action {
    const hierarchyUpdateAction = this.hierarchy.update(new DetailsNode(instance, 'projects'));
    const updateAction = new InstanceUpdateAction(this, instance, this.instanceService, this.actionState, this.logger);
    updateAction.loading$.subscribe({
      error: (err) => {
        this.logger.logError('[InstanceStoreService] update', err);
        hierarchyUpdateAction.revert();
      }
    });
    updateAction.act();
    return updateAction;
  }

  /**
   *
   * @param instanceId instance to delete
   * @returns loading status of the actual request
   */
  delete(instanceId: asiaGuid): Action {
    const hierarchyDeleteAction = this.hierarchy.delete(instanceId);
    const deleteAction = new InstanceDeleteAction(
      this,
      instanceId,
      this.instanceService,
      this.actionState,
      this.logger
    );
    deleteAction.loading$.subscribe({
      error: (err) => {
        this.logger.logError('[InstanceStoreService] delete', err);
        if (err != 'Not Found') hierarchyDeleteAction.revert();
      }
    });
    deleteAction.act();
    return deleteAction;
  }
}
