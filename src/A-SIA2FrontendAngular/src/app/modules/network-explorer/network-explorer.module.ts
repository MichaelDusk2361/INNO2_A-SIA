import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NetworkExplorerComponent } from './network-explorer.component';
import { SharedModule } from '@shared/shared.module';
import { AppRoutingModule } from 'src/app/app-routing.module';
import { NetworkExplorerInstancesComponent } from './network-explorer-instances/network-explorer-instances.component';
import { NetworkExplorerProjectsComponent } from './network-explorer-projects/network-explorer-projects.component';
import { NetworkExplorerNetworksComponent } from './network-explorer-networks/network-explorer-networks.component';
import { NetworkExplorerNetworkComponent } from './network-explorer-network/network-explorer-network.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    NetworkExplorerComponent,
    NetworkExplorerInstancesComponent,
    NetworkExplorerProjectsComponent,
    NetworkExplorerNetworksComponent,
    NetworkExplorerNetworkComponent
  ],
  imports: [CommonModule, SharedModule, AppRoutingModule, FormsModule, ReactiveFormsModule],
  exports: [NetworkExplorerComponent]
})
export class NetworkExplorerModule {}
