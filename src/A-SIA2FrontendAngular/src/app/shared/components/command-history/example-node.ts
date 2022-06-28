export interface IPosition {
  x: number;
  y: number;
}

export class ExampleNode {
  position: IPosition = { x: 0, y: 0 };
  setPosition(position: IPosition): void {
    this.position = position;
  }
}
