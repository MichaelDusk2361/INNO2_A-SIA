import { Component, ElementRef, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { SubSink } from 'subsink';
import { InputService } from '../user-input/input.service';
import { RgbToStringPipe } from './rgb-to-string.pipe';
import { faPalette } from '@fortawesome/free-solid-svg-icons';

interface RGB {
  r: number;
  g: number;
  b: number;
}
interface HSV {
  h: number;
  s: number;
  v: number;
}

@Component({
  selector: 'a-sia-color-input',
  templateUrl: './color-input.component.html',
  styleUrls: ['./color-input.component.scss']
})
export class ColorInputComponent implements OnInit, OnDestroy {
  paletteIcon = faPalette;
  choiceList: string[] = ['#00aaf4', '#4adb50', '#ffc201', '#ff561c', '#ff005d', '#b93ecc'];
  @Input() label = 'Color';
  private _color = '#00aaf4';
  public get color() {
    return this._color;
  }
  @Input()
  public set color(value: string) {
    this.selectedColorIndex = this.choiceList.findIndex((c) => c === value);
    // this blocks every call that will come from the inside
    // i.e. the user moves the handle, the _color value gets updated by the setColor function, the new color gets emitted,
    // but also read in again because the bound variable changed outside of the component
    if (value === this._color) return;
    this._color = value;
    setTimeout(() => {
      const rgb = this.RGBstringToRGB(this._color);
      const hsv = this.RGBtoHSV(rgb);
      this.setHandlePos = { left: hsv.s * 150, top: (1 - hsv.v) * 150 };
      this.createGradient(hsv.h * 360);
      this.setSliderInput((hsv.h * 360).toString());
      if (!this.selectorIsOpen) return;
      this.colorBox.style.backgroundColor = this._color;
    });
  }
  @Output() colorChanged = new EventEmitter<string>();

  setColor(color: string) {
    if (this.selectorIsOpen) this.colorBox.style.backgroundColor = this._color;
    this._color = color;
    this.colorChanged.emit(color);
  }

  colorBox!: HTMLDivElement;
  @ViewChild('colorBox') set colorBoxRef(ref: ElementRef<HTMLDivElement>) {
    if (ref) this.colorBox = ref.nativeElement;
  }

  colorHex!: HTMLSpanElement;
  @ViewChild('colorHex') set colorHexRef(ref: ElementRef<HTMLSpanElement>) {
    if (ref) this.colorHex = ref.nativeElement;
  }

  handle!: HTMLDivElement;
  @ViewChild('colorHex') set handleRef(ref: ElementRef<HTMLDivElement>) {
    if (ref) this.handle = ref.nativeElement;
  }

  gradientContainer!: HTMLDivElement;
  @ViewChild('gradientContainer') set gradientContainerRef(ref: ElementRef<HTMLDivElement>) {
    if (ref) this.gradientContainer = ref.nativeElement;
  }

  hueSlider!: HTMLInputElement;
  @ViewChild('hueSlider') set hueSliderRef(ref: ElementRef<HTMLInputElement>) {
    if (ref) this.hueSlider = ref.nativeElement;
  }

  @ViewChild('colorSelector') colorSelectorRef!: ElementRef<HTMLDivElement>;
  @ViewChild('colorSelectorIcon') colorSelectorIconRef!: ElementRef<HTMLDivElement>;

  gradientContainerContext!: HTMLDivElement;
  gradientContainerImageData!: ImageData;
  gradientContainerRect!: DOMRectReadOnly;

  gradient0Style = { opacity: 0, mixBlendMode: 'normal', zIndex: 0 };
  gradient120Style = { opacity: 0, mixBlendMode: 'normal', zIndex: 0 };
  gradient240Style = { opacity: 0, mixBlendMode: 'normal', zIndex: 0 };

  isClicked = false;

  constructor(private inputService: InputService, private rgbToStringPipe: RgbToStringPipe) {}

  getSurroundingContentSpacing(element: Element): {
    top: number;
    left: number;
    bottom: number;
    right: number;
  } {
    return {
      top:
        Number(window.getComputedStyle(element).paddingTop.slice(0, -2)) +
        Number(window.getComputedStyle(element).borderTopWidth.slice(0, -2)),
      left:
        Number(window.getComputedStyle(element).paddingLeft.slice(0, -2)) +
        Number(window.getComputedStyle(element).borderLeftWidth.slice(0, -2)),
      bottom:
        Number(window.getComputedStyle(element).paddingBottom.slice(0, -2)) +
        Number(window.getComputedStyle(element).borderBottomWidth.slice(0, -2)),
      right:
        Number(window.getComputedStyle(element).paddingRight.slice(0, -2)) +
        Number(window.getComputedStyle(element).borderRightWidth.slice(0, -2))
    };
  }
  getContentRect(element: Element, rect: DOMRectReadOnly): DOMRectReadOnly {
    const surroundingSpace = this.getSurroundingContentSpacing(element);

    const x = rect.x + surroundingSpace.left;
    const y = rect.y + surroundingSpace.top;

    const width =
      Number(window.getComputedStyle(element).width.slice(0, -2)) - surroundingSpace.left - surroundingSpace.right;
    const height =
      Number(window.getComputedStyle(element).height.slice(0, -2)) - surroundingSpace.top - surroundingSpace.bottom;

    return new DOMRectReadOnly(x, y, width, height);
  }

  subSink = new SubSink();
  ngOnInit(): void {
    this.subSink.sink = this.inputService.onPointerMove$.subscribe((e) => {
      if (!this.selectorIsOpen) return;
      this.onGlobalMouseMove(e);
    });
    this.subSink.sink = this.inputService.onPointerUp$.subscribe((e) => {
      this.isClicked = false;
    });
    this.subSink.sink = this.inputService.onPointerDown$.subscribe((e) => {
      if (
        !this.colorSelectorRef?.nativeElement.contains(e.target as Node) &&
        !this.colorSelectorIconRef?.nativeElement.contains(e.target as Node)
      )
        this.selectorIsOpen = false;
    });
    this.subSink.sink = this.inputService.onResize$.subscribe(() => {
      if (!this.selectorIsOpen) return;
      this.gradientContainerRect = this.getContentRect(
        this.gradientContainer,
        this.gradientContainer.getBoundingClientRect()
      );
    });
    this.subSink.sink = this.inputService.onScroll$.subscribe(() => {
      if (!this.selectorIsOpen) return;
      this.gradientContainerRect = this.getContentRect(
        this.gradientContainer,
        this.gradientContainer.getBoundingClientRect()
      );
    });
  }

  selectedColorIndex!: number;
  clickColor(index: number) {
    this.selectedColorIndex = index;
    this.setColor(this.choiceList[index]);
    const rgb = this.RGBstringToRGB(this._color);
    const hsv = this.RGBtoHSV(rgb);
    this.setHandlePos = { left: hsv.s * 150, top: (1 - hsv.v) * 150 };
    this.createGradient(hsv.h * 360);
    this.setSliderInput((hsv.h * 360).toString());
    if (!this.selectorIsOpen) return;
    this.colorBox.style.backgroundColor = this._color;
  }

  selectorIsOpen = false;
  onClickColorSelector() {
    if (this.selectorIsOpen) {
      this.selectorIsOpen = false;
      return;
    }
    this.selectorIsOpen = true;
    setTimeout(() => {
      this.gradientContainerRect = this.getContentRect(
        this.gradientContainer,
        this.gradientContainer.getBoundingClientRect()
      );
      const rgb = this.RGBstringToRGB(this._color);
      const hsv = this.RGBtoHSV(rgb);
      this.setHandlePos = { left: hsv.s * 150, top: (1 - hsv.v) * 150 };
      this.createGradient(hsv.h * 360);
      this.setSliderInput((hsv.h * 360).toString());
      this.colorBox.style.backgroundColor = this._color;
    });
  }

  onColorBoxMouseDown(event: MouseEvent) {
    this.isClicked = true;
    const gradientContainerMousePos = {
      left: event.x - this.gradientContainerRect.left + this.getSurroundingContentSpacing(this.gradientContainer).left,
      top: event.y - this.gradientContainerRect.top + this.getSurroundingContentSpacing(this.gradientContainer).top
    };
    this.setHandlePos = gradientContainerMousePos;
    this.setColor(this.rgbToStringPipe.transform(this.getColorFromUI()));
  }

  handlePos = { left: 0, top: 0 };
  set setHandlePos(pos: { left: number; top: number }) {
    if (!this.selectorIsOpen) return;

    this.handlePos = {
      left: Math.max(
        this.getSurroundingContentSpacing(this.gradientContainer).left,
        Math.min(
          pos.left,
          this.gradientContainerRect.width + this.getSurroundingContentSpacing(this.gradientContainer).left
        )
      ),
      top: Math.max(
        this.getSurroundingContentSpacing(this.gradientContainer).top,
        Math.min(
          pos.top,
          this.gradientContainerRect.height + this.getSurroundingContentSpacing(this.gradientContainer).top
        )
      )
    };
  }

  onHueSliderInput() {
    this.createGradient(Number(this.hueSlider?.value) ?? 0);
    this.setColor(this.rgbToStringPipe.transform(this.getColorFromUI()));
  }

  private onGlobalMouseMove(event: MouseEvent) {
    if (this.isClicked) {
      const gradientContainerMousePos = {
        left:
          event.x - this.gradientContainerRect.left + this.getSurroundingContentSpacing(this.gradientContainer).left,
        top: event.y - this.gradientContainerRect.top + this.getSurroundingContentSpacing(this.gradientContainer).top
      };
      this.setHandlePos = gradientContainerMousePos;
      this.setColor(this.rgbToStringPipe.transform(this.getColorFromUI()));
    }
  }

  private getColorFromUI(): RGB {
    this.selectedColorIndex = -1;
    return this.HSVtoRGB({
      h: Number(this.hueSlider.value),
      s: this.handlePos.left / 150,
      v: 1 - this.handlePos.top / 150
    });
  }

  public createGradient(hue: number) {
    const h = hue;
    if (h <= 60) {
      this.gradient0Style.zIndex = 1;
      this.gradient0Style.opacity = 1;
      this.gradient0Style.mixBlendMode = 'normal';
      this.gradient120Style.zIndex = 2;
      this.gradient120Style.opacity = h / 60;
      this.gradient120Style.mixBlendMode = 'lighten';
      this.gradient240Style.opacity = 0;
    } else if (h <= 120) {
      this.gradient0Style.zIndex = 2;
      this.gradient0Style.opacity = (120 - h) / 60;
      this.gradient0Style.mixBlendMode = 'lighten';
      this.gradient120Style.zIndex = 1;
      this.gradient120Style.opacity = 1;
      this.gradient120Style.mixBlendMode = 'normal';
      this.gradient240Style.opacity = 0;
    } else if (h <= 180) {
      this.gradient0Style.opacity = 0;
      this.gradient120Style.zIndex = 1;
      this.gradient120Style.opacity = 1;
      this.gradient120Style.mixBlendMode = 'normal';
      this.gradient240Style.zIndex = 2;
      this.gradient240Style.opacity = (h - 120) / 60;
      this.gradient240Style.mixBlendMode = 'lighten';
    } else if (h <= 240) {
      this.gradient0Style.opacity = 0;
      this.gradient120Style.zIndex = 2;
      this.gradient120Style.opacity = (240 - h) / 60;
      this.gradient120Style.mixBlendMode = 'lighten';
      this.gradient240Style.zIndex = 1;
      this.gradient240Style.opacity = 1;
      this.gradient240Style.mixBlendMode = 'normal';
    } else if (h <= 300) {
      this.gradient0Style.zIndex = 2;
      this.gradient0Style.opacity = (h - 240) / 60;
      this.gradient0Style.mixBlendMode = 'lighten';
      this.gradient120Style.opacity = 0;
      this.gradient240Style.zIndex = 1;
      this.gradient240Style.opacity = 1;
      this.gradient240Style.mixBlendMode = 'normal';
    } else {
      this.gradient0Style.zIndex = 1;
      this.gradient0Style.opacity = 1;
      this.gradient0Style.mixBlendMode = 'normal';
      this.gradient120Style.opacity = 0;
      this.gradient240Style.zIndex = 2;
      this.gradient240Style.opacity = (360 - h) / 60;
      this.gradient240Style.mixBlendMode = 'lighten';
    }
  }

  setSliderInput(hue: string) {
    if (!this.selectorIsOpen) return;

    this.hueSlider.value = hue;
  }

  HSVtoRGB(hsv: HSV): RGB {
    hsv.h = Math.max(0, Math.min(hsv.h, 360));
    hsv.s = Math.max(0, Math.min(hsv.s, 1));
    hsv.v = Math.max(0, Math.min(hsv.v, 1));

    const c = hsv.v * hsv.s;
    const x = c * (1 - Math.abs(((hsv.h / 60) % 2) - 1));
    const m = hsv.v - c;

    const normalizedRGB: RGB =
      hsv.h < 60
        ? { r: c, g: x, b: 0 }
        : hsv.h < 120
        ? { r: x, g: c, b: 0 }
        : hsv.h < 180
        ? { r: 0, g: c, b: x }
        : hsv.h < 240
        ? { r: 0, g: x, b: c }
        : hsv.h < 300
        ? { r: x, g: 0, b: c }
        : { r: c, g: 0, b: x };

    return {
      r: Math.round((normalizedRGB.r + m) * 255),
      g: Math.round((normalizedRGB.g + m) * 255),
      b: Math.round((normalizedRGB.b + m) * 255)
    };
  }

  RGBtoHSV(rgb: RGB): HSV {
    (rgb.r /= 255), (rgb.g /= 255), (rgb.b /= 255);

    const max = Math.max(rgb.r, rgb.g, rgb.b),
      min = Math.min(rgb.r, rgb.g, rgb.b);
    const v = max;

    const d = max - min;
    const s = max == 0 ? 0 : d / max;
    let h = 0;
    if (max == min) {
      h = 0; // achromatic
    } else {
      switch (max) {
        case rgb.r:
          h = (rgb.g - rgb.b) / d + (rgb.g < rgb.b ? 6 : 0);
          break;
        case rgb.g:
          h = (rgb.b - rgb.r) / d + 2;
          break;
        case rgb.b:
          h = (rgb.r - rgb.g) / d + 4;
          break;
      }
      h /= 6;
    }

    return { h: h, s: s, v: v };
  }

  RGBstringToRGB(rgbColor: string): RGB {
    rgbColor = rgbColor.slice(1);
    const rgb = parseInt(rgbColor, 16);
    const r = rgb >> 16;
    const g = (rgb >> 8) % 256;
    const b = rgb % 256;
    return { r: r, g: g, b: b };
  }

  ngOnDestroy(): void {
    this.subSink.unsubscribe();
  }
}
