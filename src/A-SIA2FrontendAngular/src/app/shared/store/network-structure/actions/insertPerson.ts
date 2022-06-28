import { GroupService } from '@shared/backend/group.service';
import { PersonService } from '@shared/backend/person.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaNetworkStructure } from '@shared/models/network-structure.model';
import { asiaPerson } from '@shared/models/person.Model';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { NetworkStructureStoreService } from '../network-structure-store.service';

export class NetworkStructureInsertPersonAction extends Action {
  constructor(
    private store: NetworkStructureStoreService,
    private personToInsert: asiaPerson,
    private personService: PersonService,
    private groupService: GroupService,
    private networkId: asiaGuid,
    actionsState: ActionsStateService,
    logger: LoggerService,
    private groupId?: asiaGuid
  ) {
    super(actionsState, logger);
  }
  cacheNetworkStructure!: asiaNetworkStructure;

  act() {
    this.actionsState.add(this.personToInsert.id, this);
    this.logger.logDebug('[NetworkStructureInsertPersonAction] add Person', this.personToInsert);
    this.cacheNetworkStructure = this.store._getSourceValues()!;
    const newNetworkStructure = asiaNetworkStructure.copy(this.cacheNetworkStructure);
    newNetworkStructure.people.push(this.personToInsert);
    newNetworkStructure.groups
      .find((groupEntry) => groupEntry.group.id === this.groupId)
      ?.nodes.push(this.personToInsert.id);
    this.store._setSourceValues(newNetworkStructure);
    this.personService.postPerson(this.networkId, this.personToInsert).subscribe({
      next: () => {
        if (this.groupId === undefined) {
          this.completeLoading();
          return;
        }
        this.groupService.addNodeToGroup(this.groupId, this.personToInsert.id).subscribe({
          next: () => this.completeLoading(),
          error: (err) =>
            this.failLoadingAndRevert('[NetworkStructureInsertPersonAction] Failed adding person to group', err)
        });
      },
      error: (err) => {
        this.failLoadingAndRevert('[NetworkStructureInsertPersonAction] Failed adding person', err);
      }
    });
  }

  revert() {
    this.store._setSourceValues(this.cacheNetworkStructure);
    this._loading$.complete();
  }
}
