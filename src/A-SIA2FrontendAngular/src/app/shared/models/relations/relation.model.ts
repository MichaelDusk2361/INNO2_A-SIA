import { asiaEntity, asiaGuid } from '../entity.model';
import { Guid } from 'guid-typescript';

export class asiaRelation extends asiaEntity {
  relationType: string;
  from: Guid;
  to: Guid;
  constructor(relationType: string, from: Guid, to: Guid, id?: asiaGuid) {
    super(id);
    this.relationType = relationType;
    this.from = from;
    this.to = to;
  }

  // if eslint complains about a parsing error here just ignore it.
  static override copy(relation: asiaRelation): asiaRelation {
    return new asiaRelation(relation.relationType, relation.from, relation.to, relation.id);
  }
}
