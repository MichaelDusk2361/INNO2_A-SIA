import { Injectable } from '@angular/core';
import { NetworkService } from '@shared/backend/network.service';
import { ProjectService } from '@shared/backend/project.service';
import { UserService } from '@shared/backend/user.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaNetwork } from '@shared/models/network.model';
import { DetailsNode } from '@shared/models/other/DetailsNode';
import { BehaviorSubject, Observable } from 'rxjs';
import { Action } from '../action';
import { ActionsStateService } from '../actions-state.service';
import { ProjectHierarchyStoreService } from '../project-hierarchy/project-hierarchy-store.service';
import { NetworkDeleteAction } from './actions/delete';
import { NetworkInsertAction } from './actions/insert';
import { NetworkUpdateAction } from './actions/update';
/**
 * For comments, see {@link InstanceStoreService}, which follows the same structure.
 */
@Injectable({
  providedIn: 'root'
})
export class NetworkStoreService {
  readonly _source = new BehaviorSubject<asiaNetwork[]>([]);
  get values$(): Observable<asiaNetwork[]> {
    return this._source.asObservable().pipe(this.logger.rxjsDebug('[NetworkStoreService] get values$'));
  }

  constructor(
    private networkService: NetworkService,
    private projectService: ProjectService,
    private userService: UserService,
    private logger: LoggerService,
    private hierarchy: ProjectHierarchyStoreService,
    private actionsState: ActionsStateService
  ) {
    this._load();
  }
  _load(): void {
    this.userService.getNetworks$.subscribe((res) => this._source.next(res));
  }
  _getSourceValues(): asiaNetwork[] {
    return this._source.getValue();
  }
  _setSourceValues(value: asiaNetwork[]): void {
    this._source.next(value);
  }
  /**
   * @param projectId the id of the instance which should contain the new project
   * @param network the project to add
   * @returns an observable representing the loading / error status
   */
  add(projectId: asiaGuid, network?: asiaNetwork): Action {
    if (network === undefined) network = new asiaNetwork('Unnamed Network', '');
    const hierarchyInsertAction = this.hierarchy.addNetwork(projectId, network);
    const insertAction = new NetworkInsertAction(
      this,
      this.projectService,
      projectId,
      network,
      this.actionsState,
      this.logger
    );
    insertAction.loading$.subscribe({
      error: (err) => {
        this.logger.logError('[NetworkStoreService] add', err);
        hierarchyInsertAction.revert();
      }
    });
    insertAction.act();
    return insertAction;
  }
  /**
   * @param value the updated project, be careful that the id matches the one in the backend
   * @returns an observable representing the loading / error status
   */
  update(value: asiaNetwork): Action {
    const hierarchyUpdateAction = this.hierarchy.update(new DetailsNode(value, 'network'));
    const updateAction = new NetworkUpdateAction(this, value, this.networkService, this.actionsState, this.logger);
    updateAction.loading$.subscribe({
      error: (err) => {
        this.logger.logError('[NetworkStoreService] update', err);
        hierarchyUpdateAction.revert();
      }
    });
    updateAction.act();
    return updateAction;
  }
  /**
   * @param networkId the project to delete
   * @returns an observable representing the loading / error status
   */
  delete(networkId: asiaGuid): Action {
    const hierarchyDeleteAction = this.hierarchy.delete(networkId);
    const deleteAction = new NetworkDeleteAction(this, networkId, this.networkService, this.actionsState, this.logger);
    deleteAction.loading$.subscribe({
      error: (err) => {
        this.logger.logError('[NetworkStoreService] delete', err);
        if (err != 'Not Found') hierarchyDeleteAction?.revert();
      }
    });
    deleteAction.act();
    return deleteAction;
  }

  // contains the currently open network
  _openNetwork = new BehaviorSubject<asiaNetwork | undefined>(undefined);
  get openNetwork$() {
    return this._openNetwork.asObservable().pipe(this.logger.rxjsDebug('[NetworkStoreService] get open Network'));
  }
  /**
   * Notifies everyone subscribed to the openNetwork$ observable with a network that was opened.
   * @param network the network to open
   */
  openNetwork(network: asiaNetwork) {
    this._openNetwork.next(network);
  }
}
