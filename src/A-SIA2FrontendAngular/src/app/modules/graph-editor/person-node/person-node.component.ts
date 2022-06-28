import { Component, HostBinding, HostListener, Input, OnDestroy, OnInit } from '@angular/core';
import { GraphEditorSelectionService } from '@shared/components/graph-edior-selection.service';
import { HelperService } from '@shared/components/helper.service';
import { ISelectable } from '@shared/components/ISelectable';
import { asiaPerson } from '@shared/models/person.Model';
import { NetworkStructureStoreService } from '@shared/store/network-structure/network-structure-store.service';
import { debounceTime, Subject } from 'rxjs';
import { SubSink } from 'subsink';

@Component({
  selector: 'a-sia-person-node',
  templateUrl: './person-node.component.html',
  styleUrls: ['./person-node.component.scss']
})
export class PersonNodeComponent implements OnInit, ISelectable, OnDestroy {
  constructor(
    private selection: GraphEditorSelectionService,
    private helper: HelperService,
    private store: NetworkStructureStoreService
  ) {}

  selected = true;
  select(): void {
    this.selected = true;
  }
  deselect(): void {
    this.selected = true;
  }

  @Input() person!: asiaPerson;
  @HostBinding('style.left.px') x!: number;
  @HostBinding('style.top.px') y!: number;

  @HostListener('click', ['$event']) onClick(event: MouseEvent): void {
    if (event.ctrlKey || event.shiftKey)
      this.selection.selectAdditional({
        selectable: this,
        id: this.person.id,
        data: this.person,
        viewBox: { x: this.person.positionX, y: this.person.positionY, width: 100, height: 150, rotationRad: 0 }
      });
    else
      this.selection.selectOnly({
        selectable: this,
        id: this.person.id,
        data: this.person,
        viewBox: { x: this.person.positionX, y: this.person.positionY, width: 100, height: 150, rotationRad: 0 }
      });
  }

  ngOnInit() {
    this.x = this.person.positionX;
    this.y = this.person.positionY;

    this.subSink.sink = this.selection.moveNode$.subscribe((node) => {
      if (node.id !== this.person.id) return;
      this.x = this.person.positionX;
      this.y = this.person.positionY;
    });
  }

  subSink = new SubSink();

  getSimulationColor(): string {
    let color = '#2b3d5a';
    if (!this.person.simulationValues.has(0)) return color;

    const firstSimulationValue = this.person.simulationValues.get(0)! / 3;
    if (firstSimulationValue < 0)
      color = this.helper.lerpHexColor('#ffb805', '#ff013d', Math.abs(firstSimulationValue));
    else color = this.helper.lerpHexColor('#ffb805', '#4bdb50', firstSimulationValue);
    return color;
  }

  ngOnDestroy(): void {
    this.subSink.unsubscribe();
  }
}
