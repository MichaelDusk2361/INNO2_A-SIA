import { Component, EventEmitter, Input, Output } from '@angular/core';
import { HelperService } from '@shared/components/helper.service';

@Component({
  selector: 'a-sia-graph-control-zoom',
  templateUrl: './graph-control-zoom.component.html',
  styleUrls: ['./graph-control-zoom.component.scss']
})
export class GraphControlZoomComponent {
  constructor(private helper: HelperService) {}
  @Input() set viewportScale(value: number) {
    this.viewportScaleCorrected = Math.log((8.2 / value - 1) / 81) / -0.048407;
    this._viewportScale = value;
  }
  get viewportScale(): number {
    return this._viewportScale;
  }

  @Output() controlZoomChange = new EventEmitter<number>();
  _viewportScale!: number;
  viewportScaleCorrected!: number;

  onControlZoomChange(event: Event): void {
    const newViewportScale =
      Math.round((10 * 8.2) / (1 + 81 * Math.exp(-0.048407 * (event.target as HTMLInputElement).valueAsNumber))) / 10;
    if (newViewportScale != this._viewportScale) {
      this._viewportScale = newViewportScale;
      this.controlZoomChange.emit(this._viewportScale);
    }
  }
  changeControlZoomBy(value: number): void {
    this.viewportScale = this.helper.clamp(this.viewportScale + value, 0.1, 5);
    this.controlZoomChange.emit(this._viewportScale);
  }
}
