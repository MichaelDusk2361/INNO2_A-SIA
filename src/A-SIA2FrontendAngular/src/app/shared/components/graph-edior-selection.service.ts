import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, debounceTime, filter, isEmpty, ReplaySubject, Subject } from 'rxjs';
import { GraphEditorSelectableData } from '@shared/models/other/GraphEditorSelectableData';
import { LoggerService } from './logging/logger.service';
import { ISelectable } from './ISelectable';
import { asiaPerson } from '@shared/models/person.Model';
import { SubSink } from 'subsink';
import { NetworkStructureStoreService } from '@shared/store/network-structure/network-structure-store.service';

@Injectable({
  providedIn: 'root'
})
export class GraphEditorSelectionService implements OnDestroy {
  subSink = new SubSink();
  constructor(private logger: LoggerService, private store: NetworkStructureStoreService) {
    this.subSink.sink = this.updatedPersonsDebounced.subscribe((persons) => {
      persons.forEach((person) => {
        this.store.updatePerson(person);
      });
      this.personsToUpdate = [];
    });
  }
  ngOnDestroy(): void {
    this.subSink.unsubscribe();
  }

  graphEditorCenterPosition!: { x: number; y: number };
  readonly _selected$: BehaviorSubject<GraphEditorSelectableData[]> = new BehaviorSubject(
    [] as GraphEditorSelectableData[]
  );
  public readonly selected$ = this._selected$
    .asObservable()
    .pipe(this.logger.rxjsDebug('[GraphEditorSelectionService] get values$'));

  readonly _mousePosition$: ReplaySubject<{ x: number; y: number }> = new ReplaySubject(1);
  public readonly mousePosition$ = this._mousePosition$.asObservable();

  public nextMousePosition(position: { x: number; y: number }) {
    this._mousePosition$.next(position);
  }

  readonly _moveNode$: ReplaySubject<asiaPerson> = new ReplaySubject(1);
  public readonly moveNode$ = this._moveNode$.asObservable();

  public moveNode(node: asiaPerson, delta: { x: number; y: number }) {
    node.positionX += delta.x;
    node.positionY += delta.y;
    this._moveNode$.next(node);
    const nodeToUpdateIndex = this.personsToUpdate.findIndex((p) => p.id === node.id);
    if (nodeToUpdateIndex === -1) this.personsToUpdate.push(node);
    else this.personsToUpdate[nodeToUpdateIndex] = node;
    this.updatedPersons.next(this.personsToUpdate);
  }
  personsToUpdate: asiaPerson[] = [];
  updatedPersons: Subject<asiaPerson[]> = new Subject();
  updatedPersonsDebounced = this.updatedPersons.asObservable().pipe(debounceTime(2500));

  public selectAdditional(...selections: GraphEditorSelectableData[]) {
    const currentSelections = this._selected$.getValue();
    const newSelections: GraphEditorSelectableData[] = [];
    selections.forEach((selection) => {
      const indexOfExistingSelection = currentSelections.findIndex((s) => s.id === selection.id);
      if (indexOfExistingSelection !== -1) {
        selection.selectable.deselect();
        currentSelections.splice(indexOfExistingSelection, 1);
      } else {
        selection.selectable.select();
        newSelections.push(selection);
      }
    });
    this._selected$.next([...currentSelections, ...newSelections]);
  }

  public selectOnly(...selection: GraphEditorSelectableData[]) {
    this._selected$.getValue().forEach((e) => e.selectable.deselect());
    this._selected$.next([...selection]);
    selection.forEach((e) => e.selectable.select());
  }
}
