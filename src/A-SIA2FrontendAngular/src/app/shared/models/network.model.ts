import { asiaEntity, asiaGuid } from './entity.model';

export class asiaNetwork extends asiaEntity {
  name = '';
  description = '';
  anonymous = false;
  labelType = 0;
  defaultInterval = 0;
  defaultOffset = 0;
  defaultInfluence = 0;
  minInterval = 0;
  minOffset = 0;
  minInfluence = 0;
  defaultNodevalue = 0;
  defaultReflection = 0;
  defaultPersistance = 0;
  minNodevalue = 0;
  maxNodevalue = 0;
  minReflection = 0;
  maxReflection = 0;
  minPersistance = 0;
  maxPersistance = 0;

  constructor(
    name: string,
    description?: string,
    id?: asiaGuid,
    anonymous?: boolean,
    labelType?: number,
    defaultInterval?: number,
    defaultOffset?: number,
    defaultInfluence?: number,
    minInterval?: number,
    minOffset?: number,
    minInfluence?: number,
    defaultNodevalue?: number,
    defaultReflection?: number,
    defaultPersistance?: number,
    minNodevalue?: number,
    maxNodevalue?: number,
    minReflection?: number,
    maxReflection?: number,
    minPersistance?: number,
    maxPersistance?: number
  ) {
    super(id);
    this.name = name;
    this.description = description ?? 'No description yet';
    this.anonymous = anonymous ?? false;
    this.labelType = labelType ?? 0;
    this.defaultInterval = defaultInterval ?? 0;
    this.defaultOffset = defaultOffset ?? 0;
    this.defaultInfluence = defaultInfluence ?? 0;
    this.minInterval = minInterval ?? 0;
    this.minOffset = minOffset ?? 0;
    this.minInfluence = minInfluence ?? 0;
    this.defaultNodevalue = defaultNodevalue ?? 0;
    this.defaultReflection = defaultReflection ?? 0;
    this.defaultPersistance = defaultPersistance ?? 0;
    this.minNodevalue = minNodevalue ?? 0;
    this.maxNodevalue = maxNodevalue ?? 0;
    this.minReflection = minReflection ?? 0;
    this.maxReflection = maxReflection ?? 0;
    this.minPersistance = minPersistance ?? 0;
    this.maxPersistance = maxPersistance ?? 0;
  }

  // if eslint complains about a parsing error here just ignore it.
  static override copy(network: asiaNetwork) {
    return new asiaNetwork(
      network.name,
      network.description,
      network.id,
      network.anonymous,
      network.labelType,
      network.defaultInterval,
      network.defaultOffset,
      network.defaultInfluence,
      network.minInterval,
      network.minOffset,
      network.minInfluence,
      network.defaultNodevalue,
      network.defaultReflection,
      network.defaultPersistance,
      network.minNodevalue,
      network.maxNodevalue,
      network.minReflection,
      network.maxReflection,
      network.minPersistance,
      network.maxPersistance
    );
  }
}
