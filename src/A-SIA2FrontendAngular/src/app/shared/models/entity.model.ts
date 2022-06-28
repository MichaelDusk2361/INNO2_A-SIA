import { Guid } from 'guid-typescript';

export type asiaGuid = string;
export class asiaEntity {
  id: asiaGuid;
  constructor(id?: asiaGuid) {
    if (id === undefined) id = Guid.create().toString();
    this.id = id;
  }

  static copy(entity: asiaEntity) {
    return new asiaEntity(entity.id);
  }
}
