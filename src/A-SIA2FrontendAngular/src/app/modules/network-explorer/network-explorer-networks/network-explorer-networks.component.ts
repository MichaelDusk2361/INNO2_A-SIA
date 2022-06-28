import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { asiaNetwork } from '@shared/models/network.model';
import { asiaProject } from '@shared/models/project.model';
import { UserService } from '@shared/backend/user.service';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { SubSink } from 'subsink';
import { NetworkStoreService } from '@shared/store/network/network-store.service';
import { combineLatest } from 'rxjs';
import { ProjectHierarchyStoreService } from '@shared/store/project-hierarchy/project-hierarchy-store.service';
import { DetailsNode, DetailsNodeHelper } from '@shared/models/other/DetailsNode';
import { ProjectStoreService } from '@shared/store/project/project-store.service';
import { asiaGuid } from '@shared/models/entity.model';

@Component({
  selector: 'a-sia-network-explorer-networks',
  templateUrl: './network-explorer-networks.component.html',
  styleUrls: ['./network-explorer-networks.component.scss']
})
export class NetworkExplorerNetworksComponent implements OnInit, OnDestroy {
  constructor(
    private projectStore: ProjectStoreService,
    private networkStore: NetworkStoreService,
    private hierarchy: ProjectHierarchyStoreService,
    public route: ActivatedRoute,
    public router: Router
  ) {}

  projectForm = new FormGroup({
    name: new FormControl('', [Validators.required])
  });
  editing = false;
  project!: asiaProject;
  networks: asiaNetwork[] = [];
  subs = new SubSink();

  ngOnInit(): void {
    this.subs.sink = combineLatest({
      params: this.route.params,
      hierarchy: this.hierarchy.values$
    }).subscribe((res) => {
      this.editing = false;
      const projectDetailsNode = DetailsNodeHelper.find(
        res.hierarchy,
        res.params['projectId']
      ) as DetailsNode<asiaProject>;
      this.project = projectDetailsNode.data;
      this.networks = projectDetailsNode.children.map((n) => n.data) as asiaNetwork[];
    });
  }

  submit(): void {
    this.projectStore.update({ ...this.project, name: this.projectForm.get('name')?.value });
    this.editing = false;
  }
  enableEditing(): void {
    this.editing = true;
    this.projectForm.get('name')?.setValue(this.project.name);
  }
  cancel(): void {
    this.editing = false;
  }
  delete(): void {
    this.navigateOneUp().then(() => {
      this.projectStore.delete(this.project.id);
    });
  }

  createNetwork(): void {
    this.networkStore.add(this.project.id);
  }

  navigateOneUp(): Promise<boolean> {
    const nodeToNavigateTo = this.hierarchy.navigateOneUp(this.project.id);
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
