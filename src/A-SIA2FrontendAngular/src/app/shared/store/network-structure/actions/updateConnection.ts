import { RelationService } from '@shared/backend/relation.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaNetworkStructure } from '@shared/models/network-structure.model';
import { asiaInfluencesRelation } from '@shared/models/relations/influencesRelation.model';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { NetworkStructureStoreService } from '../network-structure-store.service';

export class NetworkStructureUpdateConnectionAction extends Action {
  constructor(
    private store: NetworkStructureStoreService,
    private connectionToUpdate: asiaInfluencesRelation,
    private relationService: RelationService,
    actionsState: ActionsStateService,
    logger: LoggerService
  ) {
    super(actionsState, logger);
  }
  cacheNetworkStructure!: asiaNetworkStructure;

  act() {
    this.actionsState.add(this.connectionToUpdate.id, this);
    this.logger.logDebug('[NetworkStructureUpdateConnectionAction] update Connection', this.connectionToUpdate);
    this.cacheNetworkStructure = this.store._getSourceValues()!;
    const newNetworkStructure = asiaNetworkStructure.copy(this.cacheNetworkStructure);
    const connectionToUpdateIndex = newNetworkStructure.influenceRelations.findIndex(
      (r) => r.id == this.connectionToUpdate.id
    );
    newNetworkStructure.influenceRelations[connectionToUpdateIndex] = this.connectionToUpdate;
    this.store._setSourceValues(newNetworkStructure);
    this.relationService.putInfluencesRelation(this.connectionToUpdate).subscribe({
      next: () => this.completeLoading(),
      error: (err) =>
        this.failLoadingAndRevert('[NetworkStructureUpdateConnectionAction] Failed updating connection', err)
    });
  }

  revert() {
    this.store._setSourceValues(this.cacheNetworkStructure);
    this._loading$.complete();
  }
}
