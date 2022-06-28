import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { DetailsNode, DetailsNodeHelper, IBaseNode } from '@shared/models/other/DetailsNode';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { ProjectHierarchyStoreService } from '../project-hierarchy-store.service';

export class ProjectHierarchyInsertAction extends Action {
  cacheHierarchy!: DetailsNode<IBaseNode> | undefined;
  constructor(
    private store: ProjectHierarchyStoreService,
    private parentId: asiaGuid,
    private node: DetailsNode<IBaseNode>,
    actionsState: ActionsStateService,
    logger: LoggerService
  ) {
    super(actionsState, logger);
  }
  act(): void {
    this.cacheHierarchy = this.store._getSourceValues();
    if (this.cacheHierarchy === undefined) return;
    const newHierarchy = DetailsNodeHelper.deepCopy(this.cacheHierarchy);
    DetailsNodeHelper.find(newHierarchy, this.parentId)?.children.push(this.node);
    this.store._setSourceValues(newHierarchy);
  }
  revert(): void {
    if (this.cacheHierarchy === undefined) return;
    const newHierarchy = DetailsNodeHelper.deepCopy(this.cacheHierarchy);
    this.store._setSourceValues(newHierarchy);
  }
}
