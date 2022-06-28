import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class HelperService {
  public clamp(value: number, min: number, max: number): number {
    return Math.min(Math.max(min, value), max);
  }
  public lerp(a: number, b: number, t: number): number {
    return a * (1 - t) + b * t;
  }

  /**
   *
   * @param col1
   * @param col2
   * @param p lerp value, from 0-1
   * @returns
   */
  public lerpHexColor(col1: string, col2: string, p: number) {
    col1 = col1.slice(1);
    col2 = col2.slice(1);
    const rgb1 = parseInt(col1, 16);
    const rgb2 = parseInt(col2, 16);

    const [r1, g1, b1] = this.toArray(rgb1);
    const [r2, g2, b2] = this.toArray(rgb2);

    const q = 1 - p;
    const rr = Math.round(r2 * p + r1 * q);
    const rg = Math.round(g2 * p + g1 * q);
    const rb = Math.round(b2 * p + b1 * q);

    return `#${rr.toString(16).padStart(2, '0')}${rg.toString(16).padStart(2, '0')}${rb.toString(16).padStart(2, '0')}`;
  }

  toArray(rgb: number) {
    const r = rgb >> 16;
    const g = (rgb >> 8) % 256;
    const b = rgb % 256;

    return [r, g, b];
  }
}
