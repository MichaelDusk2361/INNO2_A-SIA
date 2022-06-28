import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { GraphEditorComponent } from '../graph-editor/graph-editor.component';
import { PaneService } from '../../pane.service';
import { NetworkStoreService } from '@shared/store/network/network-store.service';
import { take } from 'rxjs';
import { NetworkStructureStoreService } from '@shared/store/network-structure/network-structure-store.service';

@Component({
  selector: 'a-sia-network-view',
  templateUrl: './network-view.component.html',
  styleUrls: ['./network-view.component.scss']
})
export class NetworkViewComponent implements OnInit {
  constructor(
    public paneService: PaneService,
    private route: ActivatedRoute,
    private networkStore: NetworkStoreService,
    private networkStructureStore: NetworkStructureStoreService
  ) {}
  ngOnInit(): void {
    this.route.params.pipe(take(1)).subscribe((params) => {
      this.networkStore.values$.subscribe((networks) => {
        const networkToOpen = networks.find((n) => n.id === params['networkId'])!;
        this.networkStore.openNetwork(networkToOpen);
      });
    });
  }
  title = 'a-sia';

  @ViewChild('graphEditor', { read: GraphEditorComponent }) graphEditor!: GraphEditorComponent;

  onGraphEditorPaneResize(): void {
    this.graphEditor.onPaneResize();
  }
  onGraphEditorPaneInit(): void {
    this.graphEditor.onPaneInit();
  }
}
