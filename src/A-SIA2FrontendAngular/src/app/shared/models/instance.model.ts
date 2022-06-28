import { asiaEntity, asiaGuid } from './entity.model';

export class asiaInstance extends asiaEntity {
  name: string;
  description: string;
  constructor(name: string, description: string, id?: asiaGuid) {
    super(id);
    this.name = name;
    this.description = description;
  }

  // if eslint complains about a parsing error here just ignore it.
  static override copy(instance: asiaInstance) {
    return new asiaInstance(instance.name, instance.description, instance.id);
  }
}
