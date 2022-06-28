import { asiaEntity, asiaGuid } from './entity.model';

export class asiaProject extends asiaEntity {
  name: string;
  constructor(name: string, id?: asiaGuid) {
    super(id);
    this.name = name;
  }

  // if eslint complains about a parsing error here just ignore it.
  static override copy(project: asiaProject) {
    return new asiaProject(project.name, project.id);
  }
}
