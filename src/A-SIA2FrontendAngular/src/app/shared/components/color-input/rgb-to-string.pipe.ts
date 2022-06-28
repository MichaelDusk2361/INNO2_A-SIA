import { Pipe, PipeTransform } from '@angular/core';
interface RGB {
  r: number;
  g: number;
  b: number;
}

@Pipe({
  name: 'rgbToString'
})
export class RgbToStringPipe implements PipeTransform {
  transform(color: RGB, asHex = true): string {
    if (asHex) {
      const hexR = color.r < 16 ? '0' + Math.round(color.r).toString(16) : Math.round(color.r).toString(16);

      const hexG = color.g < 16 ? '0' + Math.round(color.g).toString(16) : Math.round(color.g).toString(16);

      const hexB = color.b < 16 ? '0' + Math.round(color.b).toString(16) : Math.round(color.b).toString(16);
      return ('#' + hexR + hexG + hexB).toUpperCase();
    }
    return 'rgb(' + color.r + ', ' + color.g + ', ' + color.b + ')';
  }
}
