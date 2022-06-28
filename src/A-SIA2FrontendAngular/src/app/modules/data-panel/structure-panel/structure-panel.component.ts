import { Component, ElementRef, OnInit, ViewChildren } from '@angular/core';
import { GraphEditorSelectionService } from '@shared/components/graph-edior-selection.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaGroup } from '@shared/models/group.Model';
import { DetailsNode, IBaseNode } from '@shared/models/other/DetailsNode';
import { asiaPerson } from '@shared/models/person.Model';
import { NetworkStructureStoreService } from '@shared/store/network-structure/network-structure-store.service';
import { BehaviorSubject, take } from 'rxjs';
import { SubSink } from 'subsink';

@Component({
  selector: 'a-sia-structure-panel',
  templateUrl: './structure-panel.component.html',
  styleUrls: ['./structure-panel.component.scss']
})
export class StructurePanelComponent implements OnInit {
  constructor(
    private networkStructureStore: NetworkStructureStoreService,
    private selection: GraphEditorSelectionService
  ) {}

  ngOnInit(): void {
    this.subs.sink = this.networkStructureStore.hierarchy$.subscribe((root) => {
      this.networkHierarchy = root;
    });
    this.subs.sink = this.networkStructureStore.currentNavigation$.subscribe((navigation) => {
      this.applyCurrentNavigation(navigation);
    });
  }

  subs = new SubSink();
  currentNavigation: DetailsNode<IBaseNode>[] = [];

  networkHierarchy!: DetailsNode<IBaseNode>;

  detailsElementsLoaded$ = new BehaviorSubject<ElementRef<HTMLDetailsElement>[]>([]);
  @ViewChildren('element')
  public set details(value: ElementRef<HTMLDetailsElement>[]) {
    this.detailsElementsLoaded$.next(value);
    this.networkStructureStore.currentNavigation$.pipe(take(1)).subscribe((navigation) => {
      this.applyCurrentNavigation(navigation);
    });
  }

  applyCurrentNavigation(navigation: DetailsNode<IBaseNode>[]) {
    if (navigation.length === 0) this.currentNavigation = [this.networkHierarchy];
    else this.currentNavigation = navigation;
    const elementRefs = this.detailsElementsLoaded$.getValue();
    this.currentNavigation.forEach((node, i, array) => {
      const detailsElementToOpen = elementRefs.find((element) => element.nativeElement.dataset['id'] === node.id);

      if (detailsElementToOpen === undefined) return;
      detailsElementToOpen.nativeElement.open = true;
      if (i === array.length - 1) {
        this.selectElement(detailsElementToOpen.nativeElement);
      }
    });
  }

  element!: HTMLElement;
  clickDetail(event: Event, detailsElement: HTMLElement, id: asiaGuid): void {
    event.preventDefault();
    this.selectElement(detailsElement);
    this.navigateToElement(id);
  }
  navigateToElement(id: asiaGuid) {
    this.networkStructureStore.navigate(id);
  }
  selectElement(detailsElement: HTMLElement): void {
    if (this.element == detailsElement) return;

    if (this.element != null) this.element.classList.remove('network-structure-panel__selected');
    detailsElement.classList.add('network-structure-panel__selected');
    this.element = detailsElement;
  }

  toggleDetail(detailsElement: HTMLDetailsElement): void {
    detailsElement.open = !detailsElement.open;
  }

  openDetail(detailsElement: HTMLDetailsElement): void {
    detailsElement.open = true;
  }

  addPerson() {
    const newPerson = new asiaPerson(
      'New Person',
      '',
      '#FFFFFF',
      this.selection.graphEditorCenterPosition.x,
      this.selection.graphEditorCenterPosition.y,
      new Map(),
      0,
      0,
      [],
      ''
    );
    const navigatedNode = this.currentNavigation[this.currentNavigation.length - 1];
    if (navigatedNode.data instanceof asiaGroup) this.networkStructureStore.addPerson(newPerson, navigatedNode.id);
    else this.networkStructureStore.addPerson(newPerson);
  }

  addGroup() {
    const newGroup = new asiaGroup(
      'New Group',
      '',
      '#FFFFFF',
      this.selection.graphEditorCenterPosition.x,
      this.selection.graphEditorCenterPosition.y,
      new Map(),
      0,
      0,
      true
    );
    const navigatedNode = this.currentNavigation[this.currentNavigation.length - 1];
    if (navigatedNode.data instanceof asiaGroup) this.networkStructureStore.addGroup(newGroup, navigatedNode.id);
    else this.networkStructureStore.addGroup(newGroup);
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }
}
