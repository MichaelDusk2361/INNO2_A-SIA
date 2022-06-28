import { NetworkService } from '@shared/backend/network.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaNetwork } from '@shared/models/network.model';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { NetworkStoreService } from '../network-store.service';

export class NetworkUpdateAction extends Action {
  constructor(
    private store: NetworkStoreService,
    private networkToUpdate: asiaNetwork,
    private networkService: NetworkService,
    actionsState: ActionsStateService,
    logger: LoggerService
  ) {
    super(actionsState, logger);
  }
  cacheNetworks!: asiaNetwork[];

  act() {
    this.actionsState.add(this.networkToUpdate.id, this);
    this.logger.logDebug('[NetworkUpdateAction] update network', this.networkToUpdate);
    this.cacheNetworks = this.store._getSourceValues();
    const unaffectedNetworks = this.cacheNetworks.filter((i) => i.id !== this.networkToUpdate.id);
    this.store._setSourceValues([...unaffectedNetworks, this.networkToUpdate]);
    this.networkService.putNetwork(this.networkToUpdate).subscribe({
      next: () => {
        this.completeLoading();
      },
      error: (err) => {
        this.failLoadingAndRevert('[NetworkUpdateAction] Failed updating Network', err);
      }
    });
  }

  revert() {
    this.store._setSourceValues(this.cacheNetworks);
    this._loading$.complete();
  }
}
