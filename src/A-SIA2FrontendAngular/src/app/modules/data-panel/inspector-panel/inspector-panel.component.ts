import { Component, OnDestroy, OnInit } from '@angular/core';
import { GraphEditorSelectionService } from '@shared/components/graph-edior-selection.service';
import { HelperService } from '@shared/components/helper.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaInstanceProjectNetworkRelations } from '@shared/models/instanceProjectNetworkRelations.model';
import { asiaNetworkStructure } from '@shared/models/network-structure.model';
import { asiaPerson } from '@shared/models/person.Model';
import { asiaInfluencesRelation } from '@shared/models/relations/influencesRelation.model';
import { NetworkStructureStoreService } from '@shared/store/network-structure/network-structure-store.service';
import { debounceTime, skipWhile, Subject } from 'rxjs';
import { SubSink } from 'subsink';

enum InspectionState {
  Nothing,
  Connection,
  Person
}

@Component({
  selector: 'a-sia-inspector-panel',
  templateUrl: './inspector-panel.component.html',
  styleUrls: ['./inspector-panel.component.scss']
})
export class InspectorPanelComponent implements OnInit, OnDestroy {
  constructor(
    private selection: GraphEditorSelectionService,
    private store: NetworkStructureStoreService,
    private helper: HelperService
  ) {}

  public inspectionState = InspectionState;
  network!: asiaNetworkStructure;
  subSink = new SubSink();

  ngOnInit(): void {
    this.subSink.sink = this.store.values$.subscribe((network) => {
      this.network = network;
      if (this.state === InspectionState.Person)
        this.connectedPersons = this.getAdjacentConnections(this.inspectedPerson);
    });
    this.subSink.sink = this.selection.selected$.subscribe((e) => {
      if (this.personChanged) this.updatePersonInBackend(this.inspectedPerson);

      if (e.length === 0) this.inspectNothing();
      else if (e[0].data instanceof asiaPerson) this.inspectPerson(e[0].data);
      else if (e[0].data instanceof asiaInfluencesRelation) this.inspectConnection(e[0].data);
    });
    this.subSink.sink = this.updatedPersonDebounced.subscribe((person) => {
      this.updatePersonInBackend(person);
    });
    this.subSink.sink = this.updatedConnectionDebounced.subscribe((connection) => {
      this.updateConnectionInBackend(connection);
    });
  }
  state = InspectionState.Nothing;
  inspectNothing() {
    this.state = InspectionState.Nothing;
  }

  connectionChanged = false;
  inspectedConnection!: asiaInfluencesRelation;
  personA!: asiaPerson;
  personB!: asiaPerson;
  inspectConnection(connection: asiaInfluencesRelation) {
    this.state = InspectionState.Connection;
    this.inspectedConnection = connection;
    this.personA = this.network.people.find((p) => p.id === connection.from)!;
    this.personB = this.network.people.find((p) => p.id === connection.to)!;
  }

  updateConnection(connection: { id: asiaGuid; influence?: number; interval?: string; offset?: string }) {
    this.connectionChanged = true;
    if (connection.influence !== undefined) this.inspectedConnection.influence = connection.influence;
    if (connection.interval !== undefined) this.inspectedConnection.interval = Number(connection.interval);
    if (connection.offset !== undefined) this.inspectedConnection.offset = Number(connection.offset);

    this.updatedConnection.next(this.inspectedConnection);
  }

  updateConnectionInBackend(connection: asiaInfluencesRelation) {
    this.connectionChanged = false;
    if (!this.network.influenceRelations.some((r) => r.id === connection.id)) return;
    this.store.updateConnection(connection);
  }

  deleteInspectedConnection() {
    this.state = InspectionState.Nothing;
    this.selection.selectOnly();
    this.store.deleteConnection(this.inspectedConnection);
  }

  updatedConnection: Subject<asiaInfluencesRelation> = new Subject();
  updatedConnectionDebounced = this.updatedConnection.asObservable().pipe(debounceTime(1000));

  personChanged = false;
  inspectedPerson!: asiaPerson;
  connectedPersons!: {
    incoming: { person: asiaPerson; relation: asiaInfluencesRelation }[];
    outgoing: { person: asiaPerson; relation: asiaInfluencesRelation }[];
  };
  inspectPerson(person: asiaPerson) {
    this.state = InspectionState.Person;
    this.inspectedPerson = person;
    this.connectedPersons = this.getAdjacentConnections(this.inspectedPerson);
  }
  deleteInspectedPerson() {
    this.state = InspectionState.Nothing;
    this.selection.selectOnly();
    this.store.deletePerson(this.inspectedPerson);
  }
  getAdjacentConnections(person: asiaPerson): {
    incoming: { person: asiaPerson; relation: asiaInfluencesRelation }[];
    outgoing: { person: asiaPerson; relation: asiaInfluencesRelation }[];
  } {
    const adjacentConnections: { from: asiaInfluencesRelation[]; to: asiaInfluencesRelation[] } = { from: [], to: [] };
    this.network.influenceRelations.forEach((r) => {
      if (r.from === person.id) adjacentConnections.from.push(r);
      else if (r.to === person.id) adjacentConnections.to.push(r);
    });
    const connectedPersons: {
      incoming: { person: asiaPerson; relation: asiaInfluencesRelation }[];
      outgoing: { person: asiaPerson; relation: asiaInfluencesRelation }[];
    } = { incoming: [], outgoing: [] };
    adjacentConnections.from.forEach((r) => {
      const person = this.network.people.find((p) => p.id === r.to);
      if (person !== undefined) connectedPersons.outgoing.push({ person: person, relation: r });
    });
    adjacentConnections.to.forEach((r) => {
      const person = this.network.people.find((p) => p.id === r.from);
      if (person !== undefined) connectedPersons.incoming.push({ person: person, relation: r });
    });
    return connectedPersons;
  }

  getInfluenceColor(influence: number): string {
    if (influence < 0) return this.helper.lerpHexColor('#ffb805', '#ff013d', Math.abs(influence));
    else return this.helper.lerpHexColor('#ffb805', '#4bdb50', influence);
  }

  updatePerson(person: {
    id: asiaGuid;
    name?: string;
    reflection?: string;
    persistance?: string;
    t0?: number;
    color?: string;
  }) {
    this.personChanged = true;
    if (person.name !== undefined) this.inspectedPerson.name = person.name;
    if (person.reflection !== undefined) this.inspectedPerson.reflection = Number(person.reflection);
    if (person.persistance !== undefined) this.inspectedPerson.persistance = Number(person.persistance);
    if (person.t0 !== undefined)
      setTimeout(() => {
        this.inspectedPerson.simulationValues.set(0, Number(person.t0));
      });
    if (person.color !== undefined) this.inspectedPerson.color = person.color;
    this.updatedPerson.next(this.inspectedPerson);
  }

  updatePersonInBackend(person: asiaPerson) {
    this.personChanged = false;
    if (!this.network.people.some((p) => p.id === person.id)) return;
    this.store.updatePerson(person);
  }

  updatedPerson: Subject<asiaPerson> = new Subject();
  updatedPersonDebounced = this.updatedPerson.asObservable().pipe(debounceTime(1000));

  ngOnDestroy(): void {
    this.subSink.unsubscribe();
  }
}
