import { Injectable } from '@angular/core';
import { GroupService } from '@shared/backend/group.service';
import { NetworkStructureService } from '@shared/backend/network-structure.service';
import { PersonService } from '@shared/backend/person.service';
import { RelationService } from '@shared/backend/relation.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaGroup } from '@shared/models/group.Model';
import { asiaNetworkStructure } from '@shared/models/network-structure.model';
import { asiaNetwork } from '@shared/models/network.model';
import { DetailsNode, DetailsNodeHelper, IBaseNode } from '@shared/models/other/DetailsNode';
import { asiaPerson } from '@shared/models/person.Model';
import { asiaInfluencesRelation } from '@shared/models/relations/influencesRelation.model';
import { asiaSocialNode } from '@shared/models/socialNode.model';
import { BehaviorSubject, Observable, skipWhile, take } from 'rxjs';
import { ActionsStateService } from '../actions-state.service';
import { NetworkStoreService } from '../network/network-store.service';
import { NetworkStructureDeleteConnectionAction } from './actions/deleteConnection';
import { NetworkStructureDeletePersonAction } from './actions/deletePerson';
import { NetworkStructureInsertConnectionAction } from './actions/insertConnection';
import { NetworkStructureInsertGroupAction } from './actions/insertGroup';
import { NetworkStructureInsertPersonAction } from './actions/insertPerson';
import { NetworkStructureUpdateConnectionAction } from './actions/updateConnection';
import { NetworkStructureUpdatePersonAction } from './actions/updatePerson';

/**
 * currently, this is responsible for both fetching the relations and combining them into an hierarchy, maybe this will be split up, like with the instance-project-network-relations / project-hierarchy-store.
 * but when we keep them together, it will be much easier to keep them in sync.
 */
@Injectable({
  providedIn: 'root'
})
export class NetworkStructureStoreService {
  readonly _source = new BehaviorSubject<asiaNetworkStructure | undefined>(undefined);
  get values$() {
    const temp = this._source.asObservable().pipe(
      skipWhile((n) => n === undefined),
      this.logger.rxjsDebug('[NetworkStructureStoreService] get values$')
    );
    return temp as Observable<asiaNetworkStructure>;
  }
  private openNetwork!: asiaNetwork;

  constructor(
    private logger: LoggerService,
    private networkStructureService: NetworkStructureService,
    private networkStore: NetworkStoreService,
    private personService: PersonService,
    private relationService: RelationService,
    private groupService: GroupService,
    private actionsState: ActionsStateService
  ) {
    this._load();
  }
  _load(): void {
    this.networkStore.openNetwork$.pipe(skipWhile((n) => n === undefined)).subscribe((network) => {
      network = network as asiaNetwork;
      this.openNetwork = network;
      this.networkStructureService.getNetworkStructure(network.id).subscribe((structure) => {
        this._setSourceValues(structure);
      });
    });
  }
  _getSourceValues(): asiaNetworkStructure | undefined {
    return this._source.getValue();
  }
  _setSourceValues(value: asiaNetworkStructure): void {
    this._source.next(value);

    const root = new DetailsNode<asiaNetwork>(this.openNetwork);
    const groupDetailNodes: DetailsNode<asiaGroup>[] = [];
    value.groups.map((g) => g.group).forEach((group) => groupDetailNodes.push(new DetailsNode(group)));

    const personDetailNodes: DetailsNode<asiaPerson>[] = [];
    value.people.forEach((person) => personDetailNodes.push(new DetailsNode(person)));
    const allDetailNodes: DetailsNode<asiaSocialNode>[] = (groupDetailNodes as DetailsNode<asiaSocialNode>[]).concat(
      personDetailNodes
    );
    const topLevelDetailNodes: DetailsNode<asiaSocialNode>[] = allDetailNodes;
    value.groups.forEach((groupEntry) => {
      const groupDetailsNode = groupDetailNodes.find((g) => g.id === groupEntry.group.id);
      groupEntry.nodes.forEach((childNodeId) => {
        groupDetailsNode?.children.push(
          allDetailNodes.find((n) => n.id === childNodeId) as DetailsNode<asiaSocialNode>
        );
        const nodeInTopLevelIndex = topLevelDetailNodes.findIndex((n) => n.id === childNodeId);
        if (nodeInTopLevelIndex !== -1) topLevelDetailNodes.splice(nodeInTopLevelIndex, 1);
      });
    });
    root.children.push(...topLevelDetailNodes);

    this._hierarchy.next(root);
  }

  /**
   * add person to structure and hierarchy, with optional parenting
   * @param person
   * @param groupId the group id to be parented to
   * @returns the insert person action
   */
  addPerson(person?: asiaPerson, groupId?: asiaGuid) {
    if (person === undefined) person = new asiaPerson('New Person', '', '#FFFFFF', 0, 0, new Map(), 0, 0, [], '');
    const hierarchy = this._hierarchy.getValue()!;
    const insertPersonAction = new NetworkStructureInsertPersonAction(
      this,
      person,
      this.personService,
      this.groupService,
      hierarchy.id,
      this.actionsState,
      this.logger,
      groupId
    );
    insertPersonAction.act();
    insertPersonAction.loading$.subscribe({
      error: (err) => {
        this.logger.logError('[NetworkStructureStoreService] add Person', err);
      }
    });
    return insertPersonAction;
  }

  /**
   * update person
   * @param person
   * @returns the update person action
   */
  updatePerson(person: asiaPerson) {
    const updatePersonAction = new NetworkStructureUpdatePersonAction(
      this,
      person,
      this.personService,
      this.actionsState,
      this.logger
    );
    updatePersonAction.act();
    updatePersonAction.loading$.subscribe({
      error: (err) => {
        this.logger.logError('[NetworkStructureStoreService] update Person', err);
      }
    });
    return updatePersonAction;
  }

  deletePerson(person: asiaPerson) {
    const deletePersonAction = new NetworkStructureDeletePersonAction(
      this,
      person,
      this.personService,
      this.actionsState,
      this.logger
    );
    deletePersonAction.act();
    deletePersonAction.loading$.subscribe({
      error: (err) => {
        this.logger.logError('[NetworkStructureStoreService] delete Person', err);
      }
    });
    return deletePersonAction;
  }

  addConnection(from: asiaPerson, to: asiaPerson, connection?: asiaInfluencesRelation) {
    if (connection === undefined) connection = new asiaInfluencesRelation('INFLUENCES', from.id, to.id, 0, 0, 0);
    const hierarchy = this._hierarchy.getValue()!;
    const insertPersonAction = new NetworkStructureInsertConnectionAction(
      this,
      connection,
      this.relationService,
      hierarchy.id,
      this.actionsState,
      this.logger
    );
    insertPersonAction.act();
    insertPersonAction.loading$.subscribe({
      error: (err) => {
        this.logger.logError('[NetworkStructureStoreService] add Person', err);
      }
    });
    return insertPersonAction;
  }

  updateConnection(connection: asiaInfluencesRelation) {
    const updateConnectionAction = new NetworkStructureUpdateConnectionAction(
      this,
      connection,
      this.relationService,
      this.actionsState,
      this.logger
    );
    updateConnectionAction.act();
    updateConnectionAction.loading$.subscribe({
      error: (err) => {
        this.logger.logError('[NetworkStructureStoreService] update Connection', err);
      }
    });
    return updateConnectionAction;
  }

  deleteConnection(connection: asiaInfluencesRelation) {
    const deleteConnectionAction = new NetworkStructureDeleteConnectionAction(
      this,
      connection,
      this.relationService,
      this.actionsState,
      this.logger
    );
    deleteConnectionAction.act();
    deleteConnectionAction.loading$.subscribe({
      error: (err) => {
        this.logger.logError('[NetworkStructureStoreService] delete Connection', err);
      }
    });
    return deleteConnectionAction;
  }

  /**
   * adds a group to structure and hierarchy, with optional parenting
   * @param group
   * @param groupId the group id to be parented to
   * @returns the insert group action
   */
  addGroup(group?: asiaGroup, groupId?: asiaGuid) {
    if (group === undefined) group = new asiaGroup('New Group', '', '#FFFFFF', 0, 0, new Map(), 0, 0, true);
    const hierarchy = this._hierarchy.getValue()!;
    const insertGroupAction = new NetworkStructureInsertGroupAction(
      this,
      group,
      this.groupService,
      hierarchy.id,
      this.actionsState,
      this.logger,
      groupId
    );
    insertGroupAction.act();
    insertGroupAction.loading$.subscribe({
      error: (err) => {
        this.logger.logError('[NetworkStructureStoreService] add Group', err);
      }
    });
    return insertGroupAction;
  }

  readonly _hierarchy = new BehaviorSubject<DetailsNode<IBaseNode> | undefined>(undefined);
  get hierarchy$() {
    const temp = this._hierarchy.asObservable().pipe(
      skipWhile((n) => n === undefined),
      this.logger.rxjsDebug('[NetworkStructureStoreService] get hierarchy$')
    );
    return temp as Observable<DetailsNode<IBaseNode>>;
  }

  readonly _currentNavigationSource = new BehaviorSubject<DetailsNode<IBaseNode>[]>([]);
  get currentNavigation$() {
    return this._currentNavigationSource
      .asObservable()
      .pipe(this.logger.rxjsDebug('[NetworkStructureStoreService] get navigationSource'));
  }
  navigate(id: asiaGuid): DetailsNode<IBaseNode> | undefined {
    const hierarchy = this._hierarchy.getValue();
    if (hierarchy === undefined) return;
    const path = DetailsNodeHelper.getPathTo(hierarchy, id);
    this._currentNavigationSource.next(path);
    return path[path.length - 1];
  }
  navigateOneUp(id: asiaGuid): DetailsNode<IBaseNode> | undefined {
    const hierarchy = this._hierarchy.getValue();
    if (hierarchy === undefined) return;
    const path = DetailsNodeHelper.getPathTo(hierarchy, id);
    path.splice(-1);
    this._currentNavigationSource.next(path);
    return path[path.length - 1];
  }
}
