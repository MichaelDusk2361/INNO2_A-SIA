import { Component, ElementRef, OnDestroy, OnInit, ViewChildren } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { asiaGuid } from '@shared/models/entity.model';
import { DetailsNode, IBaseNode } from '@shared/models/other/DetailsNode';
import { ProjectHierarchyStoreService } from '@shared/store/project-hierarchy/project-hierarchy-store.service';
import { BehaviorSubject, take } from 'rxjs';
import { SubSink } from 'subsink';

@Component({
  selector: 'a-sia-network-explorer',
  templateUrl: './network-explorer.component.html',
  styleUrls: ['./network-explorer.component.scss']
})
export class NetworkExplorerComponent implements OnInit, OnDestroy {
  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private projectHierarchyStore: ProjectHierarchyStoreService
  ) {}

  subs = new SubSink();
  currentNavigation: DetailsNode<IBaseNode>[] = [];
  ngOnInit(): void {
    this.projectHierarchyStore.values$.pipe(take(1)).subscribe((root) => {
      if (root.id) {
        this.router.navigate(['explorer/instances', root.id.toString()]);
        this.projectHierarchyStore.navigate(root.id);
      }
    });
    this.subs.sink = this.projectHierarchyStore.values$.subscribe((root) => {
      this.projectHierarchy = root;
    });
    this.subs.sink = this.projectHierarchyStore.currentNavigation$.subscribe((navigation) => {
      this.currentNavigation = navigation;
      const elementRefs = this.detailsElementsLoaded$.getValue();
      this.currentNavigation.forEach((node, i, array) => {
        const detailsElementToOpen = elementRefs.find((element) => element.nativeElement.dataset['id'] === node.id);

        if (detailsElementToOpen === undefined) return;
        detailsElementToOpen.nativeElement.open = true;
        if (i === array.length - 1) {
          this.selectElement(detailsElementToOpen.nativeElement);
        }
      });
    });
  }

  projectHierarchy!: DetailsNode<IBaseNode>;

  detailsElementsLoaded$ = new BehaviorSubject<ElementRef<HTMLDetailsElement>[]>([]);
  @ViewChildren('element')
  public set details(value: ElementRef<HTMLDetailsElement>[]) {
    this.detailsElementsLoaded$.next(value);
    this.projectHierarchyStore.currentNavigation$.pipe(take(1)).subscribe((navigation) => {
      this.currentNavigation = navigation;
      const elementRefs = this.detailsElementsLoaded$.getValue();
      this.currentNavigation.forEach((node, i, array) => {
        const detailsElementToOpen = elementRefs.find((element) => element.nativeElement.dataset['id'] === node.id);

        if (detailsElementToOpen === undefined) return;
        detailsElementToOpen.nativeElement.open = true;
        if (i === array.length - 1) {
          this.selectElement(detailsElementToOpen.nativeElement);
        }
      });
    });
  }

  element!: HTMLElement;
  clickDetail(event: Event, detailsElement: HTMLElement, relativeUrl: string, id: asiaGuid): void {
    event.preventDefault();
    this.selectElement(detailsElement);
    this.navigateToElement(relativeUrl, id);
  }
  navigateToElement(relativeUrl: string, id: asiaGuid) {
    this.projectHierarchyStore.navigate(id);
    this.router.navigate([relativeUrl, id], { relativeTo: this.route });
  }
  selectElement(detailsElement: HTMLElement): void {
    if (this.element == detailsElement) return;

    if (this.element != null) this.element.classList.remove('network-explorer-panel__selected');
    detailsElement.classList.add('network-explorer-panel__selected');
    this.element = detailsElement;
  }

  toggleDetail(detailsElement: HTMLDetailsElement): void {
    detailsElement.open = !detailsElement.open;
  }

  openDetail(detailsElement: HTMLDetailsElement): void {
    detailsElement.open = true;
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }
}
