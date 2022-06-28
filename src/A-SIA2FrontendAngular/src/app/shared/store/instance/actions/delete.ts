import { InstanceService } from '@shared/backend/instance.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaInstance } from '@shared/models/instance.model';
import { ignoreErrors } from '@shared/operators/ignoreErrors';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { InstanceStoreService } from '../instance-store.service';

export class InstanceDeleteAction extends Action {
  constructor(
    private store: InstanceStoreService,
    private instanceId: asiaGuid,
    private instanceService: InstanceService,
    actionsState: ActionsStateService,
    logger: LoggerService
  ) {
    super(actionsState, logger);
  }
  cacheInstances!: asiaInstance[];

  act() {
    this.actionsState.add(this.instanceId, this);
    this.cacheInstances = this.store._getSourceValues();
    this.logger.logDebug(
      '[InstanceDeleteAction] delete instance',
      this.cacheInstances.find((i) => i.id == this.instanceId)
    );
    const instances = this.cacheInstances.filter((i) => i.id !== this.instanceId);
    this.store._setSourceValues(instances);
    this.instanceService
      .deleteInstance(this.instanceId)
      .pipe(ignoreErrors('Not Found'))
      .subscribe({
        next: () => {
          this.completeLoading();
        },
        error: (err) => {
          this.failLoadingAndRevert('[InstanceDeleteAction] Failed deleting Instance', err);
        }
      });
  }

  revert() {
    this.store._setSourceValues(this.cacheInstances);
    this._loading$.complete();
  }
}
