import { asiaInstanceContainsRelation } from './relations/instanceContainsRelation.model';
import { asiaProjectContainsRelation } from './relations/projectContainsRelation.model';

export class asiaInstanceProjectNetworkRelations {
  instanceContainsRelations: asiaInstanceContainsRelation[];
  projectContainsRelations: asiaProjectContainsRelation[];
  constructor(
    instanceContainsRelations: asiaInstanceContainsRelation[],
    projectContainsRelations: asiaProjectContainsRelation[]
  ) {
    this.instanceContainsRelations = instanceContainsRelations;
    this.projectContainsRelations = projectContainsRelations;
  }

  static copy(instanceProjectNetworkRelations: asiaInstanceProjectNetworkRelations) {
    return new asiaInstanceProjectNetworkRelations(
      [...instanceProjectNetworkRelations.instanceContainsRelations.map((r) => asiaInstanceContainsRelation.copy(r))],
      [...instanceProjectNetworkRelations.projectContainsRelations.map((r) => asiaProjectContainsRelation.copy(r))]
    );
  }
}
