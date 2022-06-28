import { PersonService } from '@shared/backend/person.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaNetworkStructure } from '@shared/models/network-structure.model';
import { asiaPerson } from '@shared/models/person.Model';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { NetworkStructureStoreService } from '../network-structure-store.service';

export class NetworkStructureDeletePersonAction extends Action {
  constructor(
    private store: NetworkStructureStoreService,
    private personToDelete: asiaPerson,
    private personService: PersonService,
    actionsState: ActionsStateService,
    logger: LoggerService
  ) {
    super(actionsState, logger);
  }
  cacheNetworkStructure!: asiaNetworkStructure;

  act() {
    this.actionsState.add(this.personToDelete.id, this);
    this.logger.logDebug('[NetworkStructureDeletePersonAction] delete Person', this.personToDelete);
    this.cacheNetworkStructure = this.store._getSourceValues()!;
    const newNetworkStructure = asiaNetworkStructure.copy(this.cacheNetworkStructure);
    const personToDeleteIndex = newNetworkStructure.people.findIndex((p) => p.id === this.personToDelete.id);
    newNetworkStructure.people.splice(personToDeleteIndex, 1);
    const relationsToKeep = newNetworkStructure.influenceRelations.filter(
      (r) => r.from !== this.personToDelete.id && r.to !== this.personToDelete.id
    );
    newNetworkStructure.influenceRelations = relationsToKeep;
    this.store._setSourceValues(newNetworkStructure);
    this.personService.deletePerson(this.personToDelete.id).subscribe({
      next: () => {
        this.completeLoading();
      },
      error: (err) => {
        this.failLoadingAndRevert('[NetworkStructureDeletePersonAction] Failed deleting person', err);
      }
    });
  }

  revert() {
    this.store._setSourceValues(this.cacheNetworkStructure);
    this._loading$.complete();
  }
}
