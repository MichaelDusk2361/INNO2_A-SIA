import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { GraphEditorSelectionService } from '@shared/components/graph-edior-selection.service';
import { asiaPerson } from '@shared/models/person.Model';
import { NetworkStructureStoreService } from '@shared/store/network-structure/network-structure-store.service';
import { take } from 'rxjs';
import { SubSink } from 'subsink';

@Component({
  selector: 'a-sia-graph-editor-controls',
  templateUrl: './graph-editor-controls.component.html',
  styleUrls: ['./graph-editor-controls.component.scss']
})
export class GraphEditorControlsComponent implements OnInit, OnDestroy {
  constructor(
    private networkStructureStore: NetworkStructureStoreService,
    private selection: GraphEditorSelectionService
  ) {}

  subSink = new SubSink();

  @Input() viewportScale!: number;
  @Output() viewportScaleChange = new EventEmitter<number>();
  onControlZoomChange(value: number): void {
    this.viewportScaleChange.emit(value);
  }

  @Input() centerPosition: { x: number; y: number } = { x: 0, y: 0 };

  selectedPersons: asiaPerson[] = [];
  lastMousePosition = { x: 0, y: 0 };
  ngOnInit(): void {
    this.enableCreateConnection = false;
    this.selectedPersons = [];
    this.subSink.sink = this.selection.selected$.subscribe((selection) => {
      const cacheSelectedPersons = this.selectedPersons;
      this.selectedPersons = selection.filter((s) => s.data instanceof asiaPerson).map((s) => s.data) as asiaPerson[];
      if (this.selectedPersons.length <= 2) {
        this.enableCreateConnection = true;
      } else {
        this.enableCreateConnection = false;
      }
      if (
        this.selectedPersons.length == 1 &&
        cacheSelectedPersons.length == 1 &&
        this.selectedPersons[0].id !== cacheSelectedPersons[0].id &&
        this.activeCreateConnection
      ) {
        this.createConnection(cacheSelectedPersons[0], this.selectedPersons[0]);
      }

      if (this.selectedPersons.length == 1 && this.activeCreateConnection) this.activeCreateConnectionStatus.emit(true);
      else this.activeCreateConnectionStatus.emit(false);

      if (this.selectedPersons.length == 2 && this.activeCreateConnection) {
        this.createConnection(this.selectedPersons[0], this.selectedPersons[1]);
      }
    });
    this.subSink.sink = this.selection.mousePosition$.subscribe((pos) => {
      this.lastMousePosition = pos;
      if (!this.activeCreateConnection) return;
      if (this.selectedPersons.length === 1) {
        this.creatingConnection.emit({ person: this.selectedPersons[0], position: pos });
      }
    });
  }

  createPerson() {
    this.networkStructureStore.addPerson(
      new asiaPerson(
        'New Person1',
        '',
        '#FFFFFF',
        this.centerPosition.x,
        this.centerPosition.y,
        new Map(),
        0,
        0,
        [],
        ''
      )
    );
  }

  @Output() creatingConnection = new EventEmitter<{ person: asiaPerson; position: { x: number; y: number } }>();
  @Output() activeCreateConnectionStatus = new EventEmitter<boolean>();
  activeCreateConnection = false;
  enableCreateConnection = false;
  onCreateConnection() {
    if (!this.enableCreateConnection) return;
    if (this.activeCreateConnection) {
      this.endCreateConnection();
      return;
    }
    if (this.selectedPersons.length == 2) {
      this.createConnection(this.selectedPersons[0], this.selectedPersons[1]);
    } else {
      this.startCreateConnection();
    }
  }

  createConnection(from: asiaPerson, to: asiaPerson) {
    const personA = asiaPerson.copy(from);
    const personB = asiaPerson.copy(to);
    this.networkStructureStore.values$.pipe(take(1)).subscribe((network) => {
      if (network.influenceRelations.some((r) => r.from === personA.id && r.to === personB.id)) return;

      this.networkStructureStore.addConnection(personA, personB);
    });
  }

  startCreateConnection() {
    this.activeCreateConnection = true;
    this.activeCreateConnectionStatus.emit(true);
  }

  endCreateConnection() {
    this.activeCreateConnectionStatus.emit(false);
    this.activeCreateConnection = false;
  }

  ngOnDestroy(): void {
    this.subSink.unsubscribe();
  }
}
