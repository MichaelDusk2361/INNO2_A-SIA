import { asiaEntity, asiaGuid } from './entity.model';

export class asiaSocialNode extends asiaEntity {
  name: string;
  description: string;
  color: string;
  positionX: number;
  positionY: number;
  simulationValues: Map<number, number>;
  reflection: number;
  persistance: number;

  constructor(
    name: string,
    description: string,
    color: string,
    positionX: number,
    positionY: number,
    simulationValues: Map<number, number>,
    reflection: number,
    persistance: number,
    id?: asiaGuid
  ) {
    super(id);
    this.name = name;
    this.description = description;
    this.color = color;
    this.positionX = positionX;
    this.positionY = positionY;
    this.simulationValues = simulationValues;
    this.reflection = reflection;
    this.persistance = persistance;
    if (this.simulationValues instanceof Map) return;
    this.simulationValues = new Map(
      Object.entries(this.simulationValues).map((k) => [Number(k[0]), Number(k[1])])
    ) as Map<number, number>;
  }

  // if eslint complains about a parsing error here just ignore it.
  static override copy(socialNode: asiaSocialNode) {
    return new asiaSocialNode(
      socialNode.name,
      socialNode.description,
      socialNode.color,
      socialNode.positionX,
      socialNode.positionY,
      socialNode.simulationValues,
      socialNode.reflection,
      socialNode.persistance,
      socialNode.id
    );
  }
}
