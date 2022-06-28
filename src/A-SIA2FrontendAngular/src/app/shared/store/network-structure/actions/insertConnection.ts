import { GroupService } from '@shared/backend/group.service';
import { PersonService } from '@shared/backend/person.service';
import { RelationService } from '@shared/backend/relation.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaNetworkStructure } from '@shared/models/network-structure.model';
import { asiaPerson } from '@shared/models/person.Model';
import { asiaInfluencesRelation } from '@shared/models/relations/influencesRelation.model';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { NetworkStructureStoreService } from '../network-structure-store.service';

export class NetworkStructureInsertConnectionAction extends Action {
  constructor(
    private store: NetworkStructureStoreService,
    private connectionToInsert: asiaInfluencesRelation,
    private relationService: RelationService,
    private networkId: asiaGuid,
    actionsState: ActionsStateService,
    logger: LoggerService
  ) {
    super(actionsState, logger);
  }
  cacheNetworkStructure!: asiaNetworkStructure;

  act() {
    this.actionsState.add(this.connectionToInsert.id, this);
    this.logger.logDebug('[NetworkStructureInsertConnectionAction] insert Connection', this.connectionToInsert);
    this.cacheNetworkStructure = this.store._getSourceValues()!;
    const newNetworkStructure = asiaNetworkStructure.copy(this.cacheNetworkStructure);
    newNetworkStructure.influenceRelations.push(this.connectionToInsert);
    this.store._setSourceValues(newNetworkStructure);
    this.relationService.postInfluencesRelation(this.connectionToInsert).subscribe({
      next: () => this.completeLoading(),
      error: (err) =>
        this.failLoadingAndRevert('[NetworkStructureInsertConnectionAction] Failed inserting connection', err)
    });
  }

  revert() {
    this.store._setSourceValues(this.cacheNetworkStructure);
    this._loading$.complete();
  }
}
