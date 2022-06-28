import { ProjectService } from '@shared/backend/project.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaNetwork } from '@shared/models/network.model';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { NetworkStoreService } from '../network-store.service';

export class NetworkInsertAction extends Action {
  constructor(
    private store: NetworkStoreService,
    private projectService: ProjectService,
    private projectId: asiaGuid,
    private networkToInsert: asiaNetwork,
    actionsState: ActionsStateService,
    logger: LoggerService
  ) {
    super(actionsState, logger);
  }
  cacheNetworks!: asiaNetwork[];

  act() {
    this.actionsState.add(this.networkToInsert.id, this);
    this.logger.logDebug('[NetworkInsertAction] insert network', this.networkToInsert);
    this.cacheNetworks = this.store._getSourceValues();
    this.store._setSourceValues([...this.cacheNetworks, this.networkToInsert]);
    this.projectService.postProjectNetwork(this.projectId, this.networkToInsert).subscribe({
      next: () => {
        this.completeLoading();
      },
      error: (err) => {
        this.failLoadingAndRevert('[NetworkInsertAction] Failed inserting Network', err);
      }
    });
  }

  revert() {
    this.store._setSourceValues(this.cacheNetworks);
    this._loading$.complete();
  }
}
