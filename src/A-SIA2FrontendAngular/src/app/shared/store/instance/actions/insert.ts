import { InstanceService } from '@shared/backend/instance.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaInstance } from '@shared/models/instance.model';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { InstanceStoreService } from '../instance-store.service';

export class InstanceInsertAction extends Action {
  constructor(
    private store: InstanceStoreService,
    private instanceToInsert: asiaInstance,
    private instanceService: InstanceService,
    actionsState: ActionsStateService,
    logger: LoggerService
  ) {
    super(actionsState, logger);
  }
  cacheInstances!: asiaInstance[];

  act() {
    this.actionsState.add(this.instanceToInsert.id, this);
    this.logger.logDebug('[InstanceInsertAction] add instance', this.instanceToInsert);
    this.cacheInstances = this.store._getSourceValues();
    this.store._setSourceValues([...this.cacheInstances, this.instanceToInsert]);
    this.instanceService.postInstance(this.instanceToInsert).subscribe({
      next: () => {
        this.completeLoading();
      },
      error: (err) => {
        this.failLoadingAndRevert('[InstanceInsertAction] Failed adding instance', err);
      }
    });
  }

  revert() {
    this.store._setSourceValues(this.cacheInstances);
    this._loading$.complete();
  }
}
