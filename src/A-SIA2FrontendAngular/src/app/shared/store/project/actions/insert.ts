import { InstanceService } from '@shared/backend/instance.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaProject } from '@shared/models/project.model';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { ProjectStoreService } from '../project-store.service';

export class ProjectInsertAction extends Action {
  constructor(
    private store: ProjectStoreService,
    private instanceService: InstanceService,
    private instanceId: asiaGuid,
    private projectToInsert: asiaProject,
    actionsState: ActionsStateService,
    logger: LoggerService
  ) {
    super(actionsState, logger);
  }
  cacheProjects!: asiaProject[];
  act() {
    this.actionsState.add(this.projectToInsert.id, this);
    this.logger.logDebug('[ProjectInsertAction] insert project', this.projectToInsert);
    this.cacheProjects = this.store._getSourceValues();
    this.store._setSourceValues([...this.cacheProjects, this.projectToInsert]);
    this.instanceService.postInstanceProject(this.instanceId, this.projectToInsert).subscribe({
      next: () => {
        this.completeLoading();
      },
      error: (err) => {
        this.failLoadingAndRevert('[ProjectInsertAction] Failed inserting Project', err);
      }
    });
  }

  revert() {
    this.store._setSourceValues(this.cacheProjects);
    this._loading$.complete();
  }
}
