import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { DetailsNode, DetailsNodeHelper, IBaseNode } from '@shared/models/other/DetailsNode';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { ProjectHierarchyStoreService } from '../project-hierarchy-store.service';

export class ProjectHierarchyDeleteAction extends Action {
  cacheHierarchy!: DetailsNode<IBaseNode> | undefined;
  constructor(
    private store: ProjectHierarchyStoreService,
    private id: asiaGuid,
    actionsState: ActionsStateService,
    logger: LoggerService
  ) {
    super(actionsState, logger);
  }
  act(): void {
    this.cacheHierarchy = this.store._getSourceValues();
    if (this.cacheHierarchy === undefined) return;
    const newHierarchy = DetailsNodeHelper.deepCopy(this.cacheHierarchy);
    DetailsNodeHelper.remove(newHierarchy, this.id);
    this.store._setSourceValues(newHierarchy);
  }
  revert(): void {
    if (this.cacheHierarchy === undefined) return;
    const newHierarchy = DetailsNodeHelper.deepCopy(this.cacheHierarchy);
    this.store._setSourceValues(newHierarchy);
  }
}
