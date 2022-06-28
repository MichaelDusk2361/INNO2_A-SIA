import { asiaGuid } from './entity.model';
import { asiaSocialNode } from './socialNode.model';

export class asiaGroup extends asiaSocialNode {
  collapsed: boolean;

  constructor(
    name: string,
    description: string,
    color: string,
    positionX: number,
    positionY: number,
    simulationValues: Map<number, number>,
    reflection: number,
    persistance: number,
    collapsed: boolean,
    id?: asiaGuid
  ) {
    super(name, description, color, positionX, positionY, simulationValues, reflection, persistance, id);
    this.collapsed = collapsed;
  }

  // if eslint complains about a parsing error here just ignore it.
  static override copy(socialNode: asiaGroup): asiaGroup {
    return new asiaGroup(
      socialNode.name,
      socialNode.description,
      socialNode.color,
      socialNode.positionX,
      socialNode.positionY,
      socialNode.simulationValues,
      socialNode.reflection,
      socialNode.persistance,
      socialNode.collapsed,
      socialNode.id
    );
  }
}
