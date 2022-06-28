import { InstanceService } from '@shared/backend/instance.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaInstance } from '@shared/models/instance.model';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { InstanceStoreService } from '../instance-store.service';

export class InstanceUpdateAction extends Action {
  constructor(
    private store: InstanceStoreService,
    private instanceToUpdate: asiaInstance,
    private instanceService: InstanceService,
    actionsState: ActionsStateService,
    logger: LoggerService
  ) {
    super(actionsState, logger);
  }
  cacheInstances!: asiaInstance[];

  act() {
    this.actionsState.add(this.instanceToUpdate.id, this);
    this.logger.logDebug('[InstanceUpdateAction] update instance', this.instanceToUpdate);
    this.cacheInstances = this.store._getSourceValues();
    const unaffectedInstances = this.cacheInstances.filter((i) => i.id !== this.instanceToUpdate.id);
    this.store._setSourceValues([...unaffectedInstances, this.instanceToUpdate]);
    this.instanceService.putInstance(this.instanceToUpdate).subscribe({
      next: () => {
        this.completeLoading();
      },
      error: (err) => {
        this.failLoadingAndRevert('[InstanceUpdateAction] Failed updating Instance', err);
      }
    });
  }

  revert() {
    this.store._setSourceValues(this.cacheInstances);
    this._loading$.complete();
  }
}
