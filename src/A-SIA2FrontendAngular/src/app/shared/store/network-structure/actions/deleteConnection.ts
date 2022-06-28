import { RelationService } from '@shared/backend/relation.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaNetworkStructure } from '@shared/models/network-structure.model';
import { asiaInfluencesRelation } from '@shared/models/relations/influencesRelation.model';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { NetworkStructureStoreService } from '../network-structure-store.service';

export class NetworkStructureDeleteConnectionAction extends Action {
  constructor(
    private store: NetworkStructureStoreService,
    private connectionToDelete: asiaInfluencesRelation,
    private relationService: RelationService,
    actionsState: ActionsStateService,
    logger: LoggerService
  ) {
    super(actionsState, logger);
  }
  cacheNetworkStructure!: asiaNetworkStructure;

  act() {
    this.actionsState.add(this.connectionToDelete.id, this);
    this.logger.logDebug('[NetworkStructureDeleteConnectionAction] delete Connection', this.connectionToDelete);
    this.cacheNetworkStructure = this.store._getSourceValues()!;
    const newNetworkStructure = asiaNetworkStructure.copy(this.cacheNetworkStructure);
    newNetworkStructure.influenceRelations = newNetworkStructure.influenceRelations.filter(
      (r) => r.id !== this.connectionToDelete.id
    );
    this.store._setSourceValues(newNetworkStructure);
    this.relationService.deleteInfluencesRelation(this.connectionToDelete.id).subscribe({
      next: () => this.completeLoading(),
      error: (err) =>
        this.failLoadingAndRevert('[NetworkStructureDeleteConnectionAction] Failed deleting connection', err)
    });
  }

  revert() {
    this.store._setSourceValues(this.cacheNetworkStructure);
    this._loading$.complete();
  }
}
