import { asiaGuid } from './entity.model';
import { asiaSocialNode } from './socialNode.model';

export class asiaPerson extends asiaSocialNode {
  roles: string[];
  avatarPath: string;

  constructor(
    name: string,
    description: string,
    color: string,
    positionX: number,
    positionY: number,
    simulationValues: Map<number, number>,
    reflection: number,
    persistance: number,
    roles: string[],
    avatarPath: string,
    id?: asiaGuid
  ) {
    super(name, description, color, positionX, positionY, simulationValues, reflection, persistance, id);
    this.roles = roles;
    this.avatarPath = avatarPath;
  }

  // if eslint complains about a parsing error here just ignore it.
  static override copy(project: asiaPerson) {
    return new asiaPerson(
      project.name,
      project.description,
      project.color,
      project.positionX,
      project.positionY,
      project.simulationValues,
      project.reflection,
      project.persistance,
      project.roles,
      project.avatarPath,
      project.id
    );
  }
}
