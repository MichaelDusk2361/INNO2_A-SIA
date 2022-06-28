import { Component, OnDestroy, OnInit } from '@angular/core';
import { InputService } from '@shared/components/user-input/input.service';
import { asiaNetwork } from '@shared/models/network.model';
import { NetworkStoreService } from '@shared/store/network/network-store.service';
import { SubSink } from 'subsink';

@Component({
  selector: 'a-sia-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss']
})
export class SettingsComponent implements OnInit, OnDestroy {
  constructor(public networkStore: NetworkStoreService, public inputservice: InputService) {}

  network!: asiaNetwork;
  subSink = new SubSink();
  ngOnInit(): void {
    this.networkStore.openNetwork$.subscribe((network) => {
      this.network = network!;
    });
  }
  getShortcutNames(): string[] {
    this.inputservice.shortcuts.get('')?.shortcut.HTMLString;
    return Array.from(this.inputservice.shortcuts.keys());
  }
  onApply() {
    this.networkStore.update(this.network);
  }

  ngOnDestroy(): void {
    this.subSink.unsubscribe();
  }
}
