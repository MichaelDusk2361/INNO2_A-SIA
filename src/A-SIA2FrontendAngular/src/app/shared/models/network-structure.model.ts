import { asiaGroupEntry } from './groupEntry.model';
import { asiaPerson } from './person.Model';
import { asiaInfluencesRelation } from './relations/influencesRelation.model';

export class asiaNetworkStructure {
  people: asiaPerson[];
  influenceRelations: asiaInfluencesRelation[];
  groups: asiaGroupEntry[];
  constructor(people: asiaPerson[], influenceRelations: asiaInfluencesRelation[], groups: asiaGroupEntry[]) {
    this.people = people;
    this.influenceRelations = influenceRelations;
    this.groups = groups;
  }

  static copy(structure: asiaNetworkStructure): asiaNetworkStructure {
    return new asiaNetworkStructure(
      [...structure.people.map((p) => asiaPerson.copy(p))],
      [...structure.influenceRelations.map((r) => asiaInfluencesRelation.copy(r))],
      [...structure.groups.map((g) => asiaGroupEntry.copy(g))]
    );
  }
}
