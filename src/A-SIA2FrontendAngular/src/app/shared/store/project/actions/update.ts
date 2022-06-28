import { ProjectService } from '@shared/backend/project.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaProject } from '@shared/models/project.model';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { ProjectStoreService } from '../project-store.service';

export class ProjectUpdateAction extends Action {
  constructor(
    private store: ProjectStoreService,
    private projectToUpdate: asiaProject,
    private projectService: ProjectService,
    actionsState: ActionsStateService,
    logger: LoggerService
  ) {
    super(actionsState, logger);
  }
  cacheProjects!: asiaProject[];
  act() {
    this.actionsState.add(this.projectToUpdate.id, this);
    this.logger.logDebug('[ProjectUpdateAction] update project', this.projectToUpdate);
    this.cacheProjects = this.store._getSourceValues();
    const unaffectedProjects = this.cacheProjects.filter((i) => i.id !== this.projectToUpdate.id);
    this.store._setSourceValues([...unaffectedProjects, this.projectToUpdate]);
    this.projectService.putProject(this.projectToUpdate).subscribe({
      next: () => {
        this.completeLoading();
      },
      error: (err) => {
        this.failLoadingAndRevert('[ProjectUpdateAction] Failed updating Project', err);
      }
    });
  }

  revert() {
    this.store._setSourceValues(this.cacheProjects);
    this._loading$.complete();
  }
}
