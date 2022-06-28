import { asiaGuid } from './entity.model';
import { asiaGroup } from './group.Model';

export class asiaGroupEntry {
  group: asiaGroup;
  nodes: asiaGuid[];
  constructor(group: asiaGroup, nodes: asiaGuid[]) {
    this.group = group;
    this.nodes = nodes;
  }
  static copy(groupEntry: asiaGroupEntry) {
    return new asiaGroupEntry(asiaGroup.copy(groupEntry.group), [...groupEntry.nodes]);
  }
}
