import { Injectable } from '@angular/core';
import { AuthorizationService } from '@shared/backend/authorization.service';
import { UserService } from '@shared/backend/user.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaInstance } from '@shared/models/instance.model';
import { asiaNetwork } from '@shared/models/network.model';
import { DetailsNode, DetailsNodeHelper, IBaseNode } from '@shared/models/other/DetailsNode';
import { asiaProject } from '@shared/models/project.model';
import { BehaviorSubject, forkJoin, Observable, skipWhile, take } from 'rxjs';
import { ActionsStateService } from '../actions-state.service';
import { ProjectHierarchyDeleteAction } from './actions/delete';
import { ProjectHierarchyInsertAction } from './actions/insert';
import { ProjectHierarchyUpdateAction } from './actions/update';

/**
 * For more comments, see {@link InstanceStoreService}, which follows the same structure.
 */
@Injectable({
  providedIn: 'root'
})
export class ProjectHierarchyStoreService {
  readonly _source = new BehaviorSubject<DetailsNode<IBaseNode> | undefined>(undefined);
  get values$() {
    const temp = this._source.asObservable().pipe(
      skipWhile((n) => n === undefined),
      this.logger.rxjsDebug('[ProjectHierarchyStoreService] get values$')
    );
    return temp as Observable<DetailsNode<IBaseNode>>;
  }

  constructor(
    private logger: LoggerService,
    private userService: UserService,
    private authorizationService: AuthorizationService,
    private actionsState: ActionsStateService
  ) {
    this._load();
  }
  _load(): void {
    forkJoin({
      instances: this.userService.getInstances$,
      projects: this.userService.getProjects$,
      networks: this.userService.getNetworks$,
      relations: this.userService.getInstanceProjectNetworkRelations$,
      user: this.authorizationService.getLoggedInUser$
    })
      .pipe(take(1))
      .subscribe((res) => {
        const root = new DetailsNode({ id: res.user.id, name: 'Your Work' });
        root.url = 'instances';

        res.instances.forEach((instance) => root.children.push(new DetailsNode(instance, 'projects')));

        res.relations.instanceContainsRelations.forEach((relation) => {
          const project = res.projects.find((p) => p.id == relation.to)!;
          DetailsNodeHelper.find(root, relation.from)?.children.push(new DetailsNode(project, 'networks'));
        });

        res.relations.projectContainsRelations.forEach((relation) => {
          const network = res.networks.find((n) => n.id == relation.to)!;
          DetailsNodeHelper.find(root, relation.from)?.children.push(new DetailsNode(network, 'network'));
        });

        this._setSourceValues(root);
      });
  }
  _getSourceValues(): DetailsNode<IBaseNode> | undefined {
    return this._source.getValue();
  }
  _setSourceValues(value: DetailsNode<IBaseNode>): void {
    this._source.next(value);
  }

  addInstance(instance: asiaInstance, parentId?: asiaGuid): ProjectHierarchyInsertAction | undefined {
    const hierarchy = this._getSourceValues();
    if (hierarchy === undefined) return;
    if (parentId === undefined) parentId = hierarchy.id;
    return this.addDetailsNode(parentId, new DetailsNode<asiaInstance>(instance, 'projects'));
  }
  addProject(parentId: asiaGuid, project: asiaProject): ProjectHierarchyInsertAction {
    return this.addDetailsNode(parentId, new DetailsNode<asiaProject>(project, 'networks'));
  }
  addNetwork(parentId: asiaGuid, network: asiaNetwork): ProjectHierarchyInsertAction {
    return this.addDetailsNode(parentId, new DetailsNode<asiaNetwork>(network, 'network'));
  }
  addDetailsNode<T extends IBaseNode>(parentId: asiaGuid, node: DetailsNode<T>): ProjectHierarchyInsertAction {
    const insertAction = new ProjectHierarchyInsertAction(this, parentId, node, this.actionsState, this.logger);
    insertAction.act();
    return insertAction;
  }
  update<T extends IBaseNode>(node: DetailsNode<T>): ProjectHierarchyUpdateAction {
    const updateAction = new ProjectHierarchyUpdateAction(this, node, this.actionsState, this.logger);
    updateAction.act();
    return updateAction;
  }
  delete(id: asiaGuid): ProjectHierarchyDeleteAction {
    const deleteAction = new ProjectHierarchyDeleteAction(this, id, this.actionsState, this.logger);
    deleteAction.act();
    return deleteAction;
  }

  readonly _currentNavigationSource = new BehaviorSubject<DetailsNode<IBaseNode>[]>([]);
  get currentNavigation$() {
    return this._currentNavigationSource
      .asObservable()
      .pipe(this.logger.rxjsDebug('[ProjectHierarchyStoreService] get navigationSource'));
  }
  navigate(id: asiaGuid): DetailsNode<IBaseNode> | undefined {
    const hierarchy = this._getSourceValues();
    if (hierarchy === undefined) return;
    const path = DetailsNodeHelper.getPathTo(hierarchy, id);
    this._currentNavigationSource.next(path);
    return path[path.length - 1];
  }
  navigateOneUp(id: asiaGuid): DetailsNode<IBaseNode> | undefined {
    const hierarchy = this._getSourceValues();
    if (hierarchy === undefined) return;
    const path = DetailsNodeHelper.getPathTo(hierarchy, id);
    path.splice(-1);
    this._currentNavigationSource.next(path);
    return path[path.length - 1];
  }
}
