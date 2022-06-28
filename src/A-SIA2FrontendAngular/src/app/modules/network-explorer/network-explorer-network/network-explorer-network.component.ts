import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { asiaNetwork } from '@shared/models/network.model';
import { NetworkStoreService } from '@shared/store/network/network-store.service';
import { SubSink } from 'subsink';
import { combineLatest } from 'rxjs';
import { ProjectHierarchyStoreService } from '@shared/store/project-hierarchy/project-hierarchy-store.service';
import { DetailsNode, DetailsNodeHelper } from '@shared/models/other/DetailsNode';
import { asiaGuid } from '@shared/models/entity.model';

@Component({
  selector: 'a-sia-network-explorer-network',
  templateUrl: './network-explorer-network.component.html',
  styleUrls: ['./network-explorer-network.component.scss']
})
export class NetworkExplorerNetworkComponent implements OnInit, OnDestroy {
  constructor(
    private networkStore: NetworkStoreService,
    public route: ActivatedRoute,
    public router: Router,
    private hierarchy: ProjectHierarchyStoreService
  ) {}

  networkForm = new FormGroup({
    name: new FormControl('', [Validators.required])
  });
  editing = false;
  network!: asiaNetwork;
  subs = new SubSink();

  ngOnInit(): void {
    this.subs.sink = combineLatest({
      params: this.route.params,
      hierarchy: this.hierarchy.values$
    }).subscribe((res) => {
      this.editing = false;
      const networkDetailsNode = DetailsNodeHelper.find(
        res.hierarchy,
        res.params['networkId']
      ) as DetailsNode<asiaNetwork>;
      this.network = networkDetailsNode.data;
    });
  }

  submit(): void {
    this.networkStore.update({ ...this.network, name: this.networkForm.get('name')?.value });
    this.editing = false;
  }
  enableEditing(): void {
    this.editing = true;
    this.networkForm.get('name')?.setValue(this.network.name);
  }
  cancel(): void {
    this.editing = false;
  }
  delete(): void {
    this.navigateOneUp().then(() => {
      this.networkStore.delete(this.network.id);
    });
  }

  navigateOneUp(): Promise<boolean> {
    const nodeToNavigateTo = this.hierarchy.navigateOneUp(this.network.id);
    return this.router.navigate([nodeToNavigateTo?.url, nodeToNavigateTo?.id], { relativeTo: this.route.parent });
  }

  navigate(id: asiaGuid): Promise<boolean> {
    const nodeToNavigateTo = this.hierarchy.navigate(id);
    return this.router.navigate([nodeToNavigateTo?.url, nodeToNavigateTo?.id], { relativeTo: this.route.parent });
  }

  openNetwork(): void {
    this.router.navigate(['network', this.network.id]);
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }
}
