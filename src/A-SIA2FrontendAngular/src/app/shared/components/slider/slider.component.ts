import { OnInit, Component, ElementRef, EventEmitter, Input, Output, ViewChild, AfterViewInit } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { HelperService } from '../helper.service';

@Component({
  selector: 'a-sia-slider',
  templateUrl: './slider.component.html',
  styleUrls: ['./slider.component.scss']
})
export class SliderComponent implements AfterViewInit {
  constructor(private helper: HelperService, public sanitizer: DomSanitizer) {}

  @Input() min = 0;
  @Input() max = 10;
  @Input() minColor = '#c8d2e4';
  @Input() maxColor = '#c8d2e4';
  private _value!: number;
  public get value(): number {
    return this._value;
  }
  @Input()
  public set value(value: number) {
    if (value === this._value) return;
    this._value = value;

    if (this.value === undefined) {
      this.setValue((this.max - this.min) / 2 + this.min);
    }
    if (this.step === undefined) {
      this.step = (this.max - this.min) / 100;
    }
    setTimeout(() => {
      this.refreshNumberInputValue();
    });
    this.color = this.helper.lerpHexColor(
      this.minColor,
      this.maxColor,
      (this.value - this.min) / (this.max - this.min)
    );
  }
  @Input() step!: number;
  @Output() valueChange = new EventEmitter<number>();
  @Input() label = 'LABEL';
  @Input() for = 'FOR';
  color!: string;

  @ViewChild('numberInput') numberInputRef!: ElementRef<HTMLInputElement>;
  @ViewChild('sliderInput') sliderInputRef!: ElementRef<HTMLInputElement>;

  ngAfterViewInit(): void {
    this.sliderInputRef.nativeElement.value = this.value.toString();
  }

  getValue(eventTarget: HTMLInputElement) {
    return Number(eventTarget.value);
  }
  onNumberInput(event: Event) {
    const value = this.getValue(event.target as HTMLInputElement);
    this.setValue(value);
  }
  onSliderInput(event: Event) {
    this.setValue(this.getValue(event.target as HTMLInputElement));
    this.refreshNumberInputValue();
  }

  setValue(value: number) {
    this._value = this.helper.clamp(value, this.min, this.max);
    this.color = this.helper.lerpHexColor(
      this.minColor,
      this.maxColor,
      (this._value - this.min) / (this.max - this.min)
    );
    this.valueChange.emit(this._value);
  }

  refreshNumberInputValue() {
    this.numberInputRef.nativeElement.value = this.value.toFixed(Math.abs(Math.log10(this.step)));
  }
}
