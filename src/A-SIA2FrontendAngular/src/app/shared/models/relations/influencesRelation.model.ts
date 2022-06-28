import { asiaGuid } from '../entity.model';
import { asiaRelation } from '../relation.model';

export class asiaInfluencesRelation extends asiaRelation {
  influence: number;
  interval: number;
  offset: number;

  constructor(
    relationType: string,
    from: asiaGuid,
    to: asiaGuid,
    influence: number,
    interval: number,
    offset: number,
    id?: asiaGuid
  ) {
    super(relationType, from, to, id);
    this.influence = influence;
    this.interval = interval;
    this.offset = offset;
  }

  // if eslint complains about a parsing error here just ignore it.
  static override copy(relation: asiaInfluencesRelation) {
    return new asiaInfluencesRelation(
      relation.relationType,
      relation.from,
      relation.to,
      relation.influence,
      relation.interval,
      relation.offset,
      relation.id
    );
  }
}
