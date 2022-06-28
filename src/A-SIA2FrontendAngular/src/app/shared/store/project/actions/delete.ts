import { ProjectService } from '@shared/backend/project.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaProject } from '@shared/models/project.model';
import { ignoreErrors } from '@shared/operators/ignoreErrors';
import { Action } from '@shared/store/action';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { ProjectStoreService } from '../project-store.service';

export class ProjectDeleteAction extends Action {
  constructor(
    private store: ProjectStoreService,
    private projectId: asiaGuid,
    private projectService: ProjectService,
    actionsState: ActionsStateService,
    logger: LoggerService
  ) {
    super(actionsState, logger);
  }
  cacheProjects!: asiaProject[];
  act() {
    this.actionsState.add(this.projectId, this);
    this.cacheProjects = this.store._getSourceValues();
    this.logger.logDebug(
      '[ProjectDeleteAction] delete project',
      this.cacheProjects.find((p) => p.id === this.projectId)
    );
    const projects = this.cacheProjects.filter((n) => n.id !== this.projectId);
    this.store._setSourceValues([...projects]);
    this.projectService
      .deleteProject(this.projectId)
      .pipe(ignoreErrors('Not Found'))
      .subscribe({
        next: () => {
          this.completeLoading();
        },
        error: (err) => {
          this.failLoadingAndRevert('[ProjectDeleteAction] Failed deleting Project', err);
        }
      });
  }

  revert() {
    this.store._setSourceValues(this.cacheProjects);
    this._loading$.complete();
  }
}
