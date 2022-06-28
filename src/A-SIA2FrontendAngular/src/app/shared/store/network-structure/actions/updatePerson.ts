import { PersonService } from '@shared/backend/person.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaNetworkStructure } from '@shared/models/network-structure.model';
import { asiaPerson } from '@shared/models/person.Model';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { NetworkStructureStoreService } from '../network-structure-store.service';

export class NetworkStructureUpdatePersonAction extends Action {
  constructor(
    private store: NetworkStructureStoreService,
    private personToUpdate: asiaPerson,
    private personService: PersonService,
    actionsState: ActionsStateService,
    logger: LoggerService
  ) {
    super(actionsState, logger);
  }
  cacheNetworkStructure!: asiaNetworkStructure;

  act() {
    this.actionsState.add(this.personToUpdate.id, this);
    this.logger.logDebug('[NetworkStructureUpdatePersonAction] update Person', this.personToUpdate);
    this.cacheNetworkStructure = this.store._getSourceValues()!;
    const newNetworkStructure = asiaNetworkStructure.copy(this.cacheNetworkStructure);
    const personToUpdateIndex = newNetworkStructure.people.findIndex((p) => p.id == this.personToUpdate.id);
    newNetworkStructure.people[personToUpdateIndex] = this.personToUpdate;
    this.store._setSourceValues(newNetworkStructure);
    this.personService.putPerson(this.personToUpdate).subscribe({
      next: () => this.completeLoading(),
      error: (err) => this.failLoadingAndRevert('[NetworkStructureUpdatePersonAction] Failed updating person', err)
    });
  }

  revert() {
    this.store._setSourceValues(this.cacheNetworkStructure);
    this._loading$.complete();
  }
}
