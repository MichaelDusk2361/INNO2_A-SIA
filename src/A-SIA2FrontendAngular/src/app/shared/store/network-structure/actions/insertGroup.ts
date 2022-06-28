import { GroupService } from '@shared/backend/group.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaNetworkStructure } from '@shared/models/network-structure.model';
import { asiaGroup } from '@shared/models/group.Model';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { NetworkStructureStoreService } from '../network-structure-store.service';
import { asiaGroupEntry } from '@shared/models/groupEntry.model';

export class NetworkStructureInsertGroupAction extends Action {
  constructor(
    private store: NetworkStructureStoreService,
    private groupToInsert: asiaGroup,
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
    this.actionsState.add(this.groupToInsert.id, this);
    this.logger.logDebug('[NetworkStructureInsertGroupAction] add Group', this.groupToInsert);
    this.cacheNetworkStructure = this.store._getSourceValues()!;
    const newNetworkStructure = asiaNetworkStructure.copy(this.cacheNetworkStructure);
    newNetworkStructure.groups.push(new asiaGroupEntry(this.groupToInsert, []));
    newNetworkStructure.groups
      .find((groupEntry) => groupEntry.group.id === this.groupId)
      ?.nodes.push(this.groupToInsert.id);
    this.store._setSourceValues(newNetworkStructure);
    this.groupService.postGroup(this.networkId, this.groupToInsert).subscribe({
      next: () => {
        if (this.groupId === undefined) {
          this.completeLoading();
          return;
        }
        this.groupService.addNodeToGroup(this.groupId, this.groupToInsert.id).subscribe({
          next: () => this.completeLoading(),
          error: (err) =>
            this.failLoadingAndRevert('[NetworkStructureInsertGroupAction] Failed adding person to group', err)
        });
      },
      error: (err) => {
        this.failLoadingAndRevert('[NetworkStructureInsertGroupAction] Failed adding group', err);
      }
    });
  }

  revert() {
    this.store._setSourceValues(this.cacheNetworkStructure);
    this._loading$.complete();
  }
}
