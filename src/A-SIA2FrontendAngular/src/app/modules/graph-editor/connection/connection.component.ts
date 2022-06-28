import { Component, HostBinding, HostListener, Input, OnInit } from '@angular/core';
import { GraphEditorSelectionService } from '@shared/components/graph-edior-selection.service';
import { HelperService } from '@shared/components/helper.service';
import { ISelectable } from '@shared/components/ISelectable';
import { asiaPerson } from '@shared/models/person.Model';
import { asiaInfluencesRelation } from '@shared/models/relations/influencesRelation.model';

@Component({
  selector: 'a-sia-connection',
  templateUrl: './connection.component.html',
  styleUrls: ['./connection.component.scss']
})
export class ConnectionComponent implements OnInit, ISelectable {
  constructor(private selection: GraphEditorSelectionService, private helper: HelperService) {}

  selected = false;
  select(): void {
    this.selected = true;
  }
  deselect(): void {
    this.selected = false;
  }

  @Input() connection!: asiaInfluencesRelation;
  @Input() personA!: asiaPerson;
  @Input() personB!: asiaPerson;
  @Input() set tempEndPosition(pos: { x: number; y: number }) {
    this.personB.positionX = pos.x;
    this.personB.positionY = pos.y;
    this.setTransform();
  }
  flipText = false;

  @HostBinding('style.left.px') x!: number;
  @HostBinding('style.top.px') y!: number;
  @HostBinding('style.width.px') width!: number;
  @HostBinding('style.transform') angleTransform!: string;

  @HostListener('click', ['$event']) onClick(event: MouseEvent): void {
    event.stopPropagation();
    const width = this.getWidth();
    const rotation = this.getRotation();
    const position = this.getPosition();
    if (event.ctrlKey || event.shiftKey)
      this.selection.selectAdditional({
        selectable: this,
        id: this.connection.id,
        data: this.connection,
        viewBox: { x: position.x, y: position.y, width: width, height: 40, rotationRad: rotation }
      });
    else
      this.selection.selectOnly({
        selectable: this,
        id: this.connection.id,
        data: this.connection,
        viewBox: { x: position.x, y: position.y, width: width, height: 40, rotationRad: rotation }
      });
  }
  color!: string;

  getWidth() {
    const deltaX = this.personB.positionX - this.personA.positionX;
    const deltaY = this.personB.positionY - this.personA.positionY;
    return Math.sqrt(Math.pow(deltaX, 2) + Math.pow(deltaY, 2));
  }

  getRotation() {
    const deltaX = this.personB.positionX - this.personA.positionX;
    const deltaY = this.personB.positionY - this.personA.positionY;
    return Math.atan2(deltaY, deltaX);
  }

  getPosition(): { x: number; y: number } {
    return { x: this.personA.positionX + 50, y: this.personA.positionY };
  }

  ngOnInit() {
    this.setTransform();

    this.selection.moveNode$.subscribe((node) => {
      if (node.id !== this.personA.id && node.id !== this.personB.id) return;
      this.setTransform();
    });
  }

  setTransform() {
    const position = this.getPosition();
    this.x = position.x;
    this.y = position.y;
    this.width = this.getWidth();
    const rotation = this.getRotation();

    this.angleTransform = `rotate(${rotation}rad)`;
    if (Math.abs(rotation) > Math.PI / 2) this.flipText = true;
  }

  getInfluenceColor(): string {
    if (this.connection.influence < 0)
      this.color = this.helper.lerpHexColor('#ffb805', '#ff013d', Math.abs(this.connection.influence));
    else this.color = this.helper.lerpHexColor('#ffb805', '#4bdb50', this.connection.influence);
    return this.color;
  }
}
