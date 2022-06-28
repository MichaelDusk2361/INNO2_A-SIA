import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, NavigationEnd, NavigationStart, Router } from '@angular/router';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaInstance } from '@shared/models/instance.model';
import { DetailsNode, DetailsNodeHelper, IBaseNode } from '@shared/models/other/DetailsNode';
import { ActionsStateService, ActionState } from '@shared/store/actions-state.service';
import { InstanceStoreService } from '@shared/store/instance/instance-store.service';
import { ProjectHierarchyStoreService } from '@shared/store/project-hierarchy/project-hierarchy-store.service';
import { combineLatest } from 'rxjs';
import { SubSink } from 'subsink';

@Component({
  selector: 'a-sia-network-explorer-instances',
  templateUrl: './network-explorer-instances.component.html',
  styleUrls: ['./network-explorer-instances.component.scss']
})
export class NetworkExplorerInstancesComponent implements OnInit, OnDestroy {
  constructor(
    private hierarchy: ProjectHierarchyStoreService,
    public route: ActivatedRoute,
    private instanceStore: InstanceStoreService,
    private router: Router,
    private actionsState: ActionsStateService
  ) {}

  subs = new SubSink();
  instances: asiaInstance[] = [];
  ngOnInit(): void {
    this.subs.sink = combineLatest({
      params: this.route.params,
      hierarchy: this.hierarchy.values$
    }).subscribe((res) => {
      const instanceDetailsNodes = DetailsNodeHelper.find(
        res.hierarchy,
        res.params['userId']
      ) as DetailsNode<IBaseNode>;
      this.instances = instanceDetailsNodes.children.map((n) => n.data) as asiaInstance[];
    });
  }

  createInstance(): void {
    this.instanceStore.add();
  }

  navigate(id: asiaGuid): Promise<boolean> {
    const nodeToNavigateTo = this.hierarchy.navigate(id);
    return this.router.navigate([nodeToNavigateTo?.url, nodeToNavigateTo?.id], { relativeTo: this.route.parent });
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }
}
