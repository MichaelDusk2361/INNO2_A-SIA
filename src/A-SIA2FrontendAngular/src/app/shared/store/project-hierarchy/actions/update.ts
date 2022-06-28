import { LoggerService } from '@shared/components/logging/logger.service';
import { DetailsNode, DetailsNodeHelper, IBaseNode } from '@shared/models/other/DetailsNode';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { ProjectHierarchyStoreService } from '../project-hierarchy-store.service';

export class ProjectHierarchyUpdateAction extends Action {
  cacheHierarchy!: DetailsNode<IBaseNode> | undefined;
  constructor(
    private store: ProjectHierarchyStoreService,
    private node: DetailsNode<IBaseNode>,
    actionsState: ActionsStateService,
    logger: LoggerService
  ) {
    super(actionsState, logger);
  }
  act(): void {
    this.cacheHierarchy = this.store._getSourceValues();
    if (this.cacheHierarchy === undefined) return;
    const hierarchy = DetailsNodeHelper.deepCopy(this.cacheHierarchy);
    const nodeToUpdate = DetailsNodeHelper.find(hierarchy, this.node.id);
    if (nodeToUpdate === undefined) return;
    nodeToUpdate.name = this.node.name;
    nodeToUpdate.data = this.node.data;
    this.store._setSourceValues(hierarchy);
  }
  revert(): void {
    if (this.cacheHierarchy === undefined) return;
    const newHierarchy = DetailsNodeHelper.deepCopy(this.cacheHierarchy);
    this.store._setSourceValues(newHierarchy);
  }
}
