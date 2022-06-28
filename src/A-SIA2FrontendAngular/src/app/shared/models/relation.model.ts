import { asiaEntity, asiaGuid } from './entity.model';
import { Guid } from 'guid-typescript';

export class asiaRelation extends asiaEntity {
  relationType: string;
  from: asiaGuid;
  to: asiaGuid;
  constructor(relationType: string, from: asiaGuid, to: asiaGuid, id?: asiaGuid) {
    super(id);
    this.relationType = relationType;
    this.from = from;
    this.to = to;
  }

  // if eslint complains about a parsing error here just ignore it.
  static override copy(relation: asiaRelation) {
    return new asiaRelation(relation.relationType, relation.from, relation.to, relation.id);
  }
}
