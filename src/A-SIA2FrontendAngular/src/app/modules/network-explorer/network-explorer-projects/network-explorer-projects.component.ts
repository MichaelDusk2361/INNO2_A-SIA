import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { asiaInstance } from '@shared/models/instance.model';
import { asiaProject } from '@shared/models/project.model';
import { combineLatest } from 'rxjs';
import { ProjectHierarchyStoreService } from '@shared/store/project-hierarchy/project-hierarchy-store.service';
import { DetailsNode, DetailsNodeHelper } from '@shared/models/other/DetailsNode';
import { InstanceStoreService } from '@shared/store/instance/instance-store.service';
import { ProjectStoreService } from '@shared/store/project/project-store.service';
import { SubSink } from 'subsink';
import { asiaGuid } from '@shared/models/entity.model';
import { ActionsStateService, ActionState } from '@shared/store/actions-state.service';

@Component({
  selector: 'a-sia-network-explorer-projects',
  templateUrl: './network-explorer-projects.component.html',
  styleUrls: ['./network-explorer-projects.component.scss']
})
export class NetworkExplorerProjectsComponent implements OnInit, OnDestroy {
  constructor(
    private instanceStore: InstanceStoreService,
    private projectStore: ProjectStoreService,
    public route: ActivatedRoute,
    private hierarchy: ProjectHierarchyStoreService,
    private router: Router,
    private actionsState: ActionsStateService
  ) {}

  instanceForm = new FormGroup({
    name: new FormControl('', [Validators.required])
  });
  editing = false;
  instance!: asiaInstance;
  projects!: asiaProject[];
  subs = new SubSink();
  loading = false;

  ngOnInit(): void {
    this.subs.sink = combineLatest({
      params: this.route.params,
      hierarchy: this.hierarchy.values$
    }).subscribe((res) => {
      this.editing = false;
      const instanceDetailsNode = DetailsNodeHelper.find(
        res.hierarchy,
        res.params['instanceId']
      ) as DetailsNode<asiaInstance>;
      this.instance = instanceDetailsNode.data;
      this.projects = instanceDetailsNode.children.map((n) => n.data) as asiaProject[];
    });

    this.subs.sink = this.route.params.subscribe((params) => {
      // yes, this subscribes multiple times when navigating between "explorer/projects" pages, but it's to easy to ignore. And to hard to fix.
      this.subs.sink = this.actionsState.getState(params['instanceId']).subscribe((state) => {
        this.loading = state === ActionState.Locked;
      });
    });
  }

  submit(): void {
    this.instanceStore.update({ ...this.instance, name: this.instanceForm.get('name')?.value });
    this.editing = false;
  }
  enableEditing(): void {
    this.editing = true;
    this.instanceForm.get('name')?.setValue(this.instance.name);
  }
  cancel(): void {
    this.editing = false;
  }
  delete(): void {
    this.navigateOneUp().then(() => {
      this.instanceStore.delete(this.instance.id);
    });
  }

  createProject(): void {
    this.projectStore.add(this.instance.id);
  }

  navigateOneUp(): Promise<boolean> {
    const nodeToNavigateTo = this.hierarchy.navigateOneUp(this.instance.id);
    return this.router.navigate([nodeToNavigateTo?.url, nodeToNavigateTo?.id], { relativeTo: this.route.parent });
  }

  navigate(id: asiaGuid): Promise<boolean> {
    const nodeToNavigateTo = this.hierarchy.navigate(id);
    return this.router.navigate([nodeToNavigateTo?.url, nodeToNavigateTo?.id], { relativeTo: this.route.parent });
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }
}
