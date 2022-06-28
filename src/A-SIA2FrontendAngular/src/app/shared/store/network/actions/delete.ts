import { NetworkService } from '@shared/backend/network.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaNetwork } from '@shared/models/network.model';
import { ignoreErrors } from '@shared/operators/ignoreErrors';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { NetworkStoreService } from '../network-store.service';

export class NetworkDeleteAction extends Action {
  constructor(
    private store: NetworkStoreService,
    private networkId: asiaGuid,
    private networkService: NetworkService,
    actionsState: ActionsStateService,
    logger: LoggerService
  ) {
    super(actionsState, logger);
  }
  cacheNetworks!: asiaNetwork[];

  act() {
    this.actionsState.add(this.networkId, this);
    this.cacheNetworks = this.store._getSourceValues();
    this.logger.logDebug(
      '[NetworkDeleteAction] delete network',
      this.cacheNetworks.find((n) => n.id === this.networkId)
    );
    const networks = this.cacheNetworks.filter((n) => n.id !== this.networkId);
    this.store._setSourceValues([...networks]);
    this.networkService
      .deleteNetwork(this.networkId)
      .pipe(ignoreErrors('Not Found'))
      .subscribe({
        next: () => {
          this.completeLoading();
        },
        error: (err) => {
          this.failLoadingAndRevert('[NetworkDeleteAction] Failed deleting Network', err);
        }
      });
  }

  revert() {
    this.store._setSourceValues(this.cacheNetworks);
    this._loading$.complete();
  }
}
