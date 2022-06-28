import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ModalService } from '@shared/components/modal/modal.service';
import { asiaNetwork } from '@shared/models/network.model';
import { NetworkStoreService } from '@shared/store/network/network-store.service';
import { SubSink } from 'subsink';

@Component({
  selector: 'a-sia-network-panel',
  templateUrl: './network-panel.component.html',
  styleUrls: ['./network-panel.component.scss']
})
export class NetworkPanelComponent implements OnInit, OnDestroy {
  constructor(public networkStore: NetworkStoreService, private modalService: ModalService) {}
  openNetwork!: asiaNetwork;
  loading = true;
  subs = new SubSink();
  ngOnInit(): void {
    this.subs.sink = this.networkStore.openNetwork$.subscribe((res) => {
      if (res === undefined) return;
      this.loading = false;
      this.openNetwork = res;
    });
  }
  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }
}
