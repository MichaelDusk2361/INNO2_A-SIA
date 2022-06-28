import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { NetworkExplorerInstancesComponent } from './modules/network-explorer/network-explorer-instances/network-explorer-instances.component';
import { NetworkExplorerNetworkComponent } from './modules/network-explorer/network-explorer-network/network-explorer-network.component';
import { NetworkExplorerNetworksComponent } from './modules/network-explorer/network-explorer-networks/network-explorer-networks.component';
import { NetworkExplorerProjectsComponent } from './modules/network-explorer/network-explorer-projects/network-explorer-projects.component';
import { NetworkExplorerComponent } from './modules/network-explorer/network-explorer.component';
import { NetworkViewComponent } from './modules/network-view/network-view.component';

const routes: Routes = [
  { path: '', redirectTo: '/explorer', pathMatch: 'full' },
  { path: 'network/:networkId', component: NetworkViewComponent },
  {
    path: 'explorer',
    component: NetworkExplorerComponent,
    children: [
      {
        path: 'instances/:userId',
        component: NetworkExplorerInstancesComponent
      },
      {
        path: 'projects/:instanceId',
        component: NetworkExplorerProjectsComponent
      },
      {
        path: 'networks/:projectId',
        component: NetworkExplorerNetworksComponent
      },
      {
        path: 'network/:networkId',
        component: NetworkExplorerNetworkComponent
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
