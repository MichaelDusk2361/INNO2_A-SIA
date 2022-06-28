import { Injectable } from '@angular/core';
import { InstanceService } from '@shared/backend/instance.service';
import { ProjectService } from '@shared/backend/project.service';
import { UserService } from '@shared/backend/user.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { DetailsNode } from '@shared/models/other/DetailsNode';
import { asiaProject } from '@shared/models/project.model';
import { BehaviorSubject, Observable } from 'rxjs';
import { Action } from '../action';
import { ActionsStateService } from '../actions-state.service';
import { ProjectHierarchyStoreService } from '../project-hierarchy/project-hierarchy-store.service';
import { ProjectDeleteAction } from './actions/delete';
import { ProjectInsertAction } from './actions/insert';
import { ProjectUpdateAction } from './actions/update';

/**
 * For comments, see {@link InstanceStoreService}, which follows the same structure.
 */
@Injectable({
  providedIn: 'root'
})
export class ProjectStoreService {
  readonly _source = new BehaviorSubject<asiaProject[]>([]);
  get values$(): Observable<asiaProject[]> {
    return this._source.asObservable().pipe(this.logger.rxjsDebug('[ProjectStoreService] get values$'));
  }

  constructor(
    private projectService: ProjectService,
    private instanceService: InstanceService,
    private userService: UserService,
    private hierarchy: ProjectHierarchyStoreService,
    private logger: LoggerService,
    private actionsState: ActionsStateService
  ) {
    this._load();
  }
  _load(): void {
    this.userService.getProjects$.subscribe((res) => this._source.next(res));
  }
  _getSourceValues(): asiaProject[] {
    return this._source.getValue();
  }
  _setSourceValues(value: asiaProject[]): void {
    this._source.next(value);
  }

  /**
   * @param instanceId the id of the instance which should contain the new project
   * @param project the project to add
   * @returns an observable representing the loading / error status
   */
  add(instanceId: asiaGuid, project?: asiaProject): Action {
    if (project === undefined) project = new asiaProject('Unnamed Project');
    const hierarchyInsertAction = this.hierarchy.addProject(instanceId, project);
    const insertAction = new ProjectInsertAction(
      this,
      this.instanceService,
      instanceId,
      project,
      this.actionsState,
      this.logger
    );
    insertAction.loading$.subscribe({
      error: (err) => {
        this.logger.logError('[ProjectStoreService] add', err);
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
  update(value: asiaProject): Action {
    const hierarchyUpdateAction = this.hierarchy.update(new DetailsNode(value, 'networks'));
    const updateAction = new ProjectUpdateAction(this, value, this.projectService, this.actionsState, this.logger);
    updateAction.loading$.subscribe({
      error: (err) => {
        this.logger.logError('[ProjectStoreService] update', err);
        hierarchyUpdateAction.revert();
      }
    });
    updateAction.act();
    return updateAction;
  }

  /**
   * @param projectId the project to delete
   * @returns an observable representing the loading / error status
   */
  delete(projectId: asiaGuid): Action {
    const hierarchyDeleteAction = this.hierarchy.delete(projectId);
    const deleteAction = new ProjectDeleteAction(this, projectId, this.projectService, this.actionsState, this.logger);
    deleteAction.loading$.subscribe({
      error: (err) => {
        this.logger.logError('[ProjectStoreService] delete', err);
        if (err != 'Not Found') hierarchyDeleteAction?.revert();
      }
    });
    deleteAction.act();
    return deleteAction;
  }
}
