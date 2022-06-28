import { Component, ElementRef, HostListener, OnDestroy, OnInit } from '@angular/core';
import { GraphEditorSelectionService } from '@shared/components/graph-edior-selection.service';
import { HelperService } from '@shared/components/helper.service';
import { InputService } from '@shared/components/user-input/input.service';
import { Shortcut } from '@shared/components/user-input/shortcut';
import { SelectableViewBox } from '@shared/models/other/GraphEditorSelectableData';
import { asiaPerson } from '@shared/models/person.Model';
import { asiaInfluencesRelation } from '@shared/models/relations/influencesRelation.model';
import { NetworkStructureStoreService } from '@shared/store/network-structure/network-structure-store.service';
import { Guid } from 'guid-typescript';
import { SubSink } from 'subsink';

/**
 * Top level component for the Graph Editor.
 * Consists of the actual editor, which is referred to as "Viewport" throughout the code - and an overlay containing tools.
 * If this component gets to chunky, it should be split into a viewport and an overlay component
 *
 * # Features
 *
 * ## Panning
 *
 * Panning is achieved by absolute positioning and modifying the transform through translate(x,y). positioning is
 * relative to the top left corner, and goes from (0,0) to (-graphEditorViewport.width, -graphEditorViewport.height).
 * The Viewport has a fixed size, and is has bounds on all side. The whole thing is a bit more complicated
 * than it could be, because itself is in a resizable container (aka the pane). This adds a couple of edge-cases to bounds-checking.
 * It currently has no scrollbars, because html scrolling does not work out of the box, and we would have to implement our
 * own scrollbars, which may become a future thing.
 */
@Component({
  selector: 'a-sia-graph-editor',
  templateUrl: './graph-editor.component.html',
  styleUrls: ['./graph-editor.component.scss']
})
export class GraphEditorComponent implements OnInit, OnDestroy {
  constructor(
    private inputService: InputService,
    private element: ElementRef,
    private helper: HelperService,
    private networkStructureStore: NetworkStructureStoreService,
    private selection: GraphEditorSelectionService
  ) {}
  selectionViewBoxes: SelectableViewBox[] = [];
  personNodes: asiaPerson[] = [];
  connectedPersons: Map<asiaInfluencesRelation, { a: asiaPerson; b: asiaPerson }> = new Map();
  subSink = new SubSink();
  graphEditorViewport = {
    canBePanned: false,
    width: 14000,
    height: 10000,
    minScale: 0.1,
    maxScale: 5,
    transform: 'translate(0px,0px)',
    position: { x: 0, y: 0 },
    transitionInterpolation: false,
    scale: 1,
    centerPosition: { x: 0, y: 0 }
  };

  /**
   * Viewport scale for the zoom control, needs to be updated so that
   * the zoom control can adjust the slider without causing a binding loop.
   * (e.g. using graphEditorViewport.scale as binding leads to many unnecessary
   * change detections as control and graph-editor are trying to update each other again and again)
   */
  viewportScale = 1;

  /**
   * Constrains graphEditorViewport.position between (0,0) and
   * (-this.graphEditorViewport.width + this.boundingClientRect.width, -this.graphEditorViewport.height + this.boundingClientRect.height)
   */
  boundsCheckAndConstrain(): void {
    this.graphEditorViewport.scale = this.helper.clamp(
      this.graphEditorViewport.scale,
      this.graphEditorViewport.minScale,
      this.graphEditorViewport.maxScale
    );
    this.graphEditorViewport.position.x = this.helper.clamp(
      this.graphEditorViewport.position.x,
      -this.graphEditorViewport.width + this.boundingClientRect.width / this.graphEditorViewport.scale,
      0
    );
    this.graphEditorViewport.position.y = this.helper.clamp(
      this.graphEditorViewport.position.y,
      -this.graphEditorViewport.height + this.boundingClientRect.height / this.graphEditorViewport.scale,
      0
    );
  }

  /**
   * Applies graphEditorViewport.position to graphEditorViewport.transform with string interpolation
   */
  applyEditorViewportPositionAndScale(): void {
    this.graphEditorViewport.transform = `scale(${this.graphEditorViewport.scale}) translate(${this.graphEditorViewport.position.x}px, ${this.graphEditorViewport.position.y}px)`;
  }

  /**
   * Subscribes to global events for user interaction
   */
  ngOnInit(): void {
    this.selection.graphEditorCenterPosition = this.graphEditorViewport.centerPosition;
    this.subSink.sink = this.inputService.onResize$.subscribe(() => {
      this.onPaneResize();
    });
    this.subSink.sink = this.inputService.onPointerMove$.subscribe((e) => {
      this.onGlobalPointerMove(e);
    });
    this.subSink.sink = this.inputService.onPointerUp$.subscribe(() => {
      this.onGlobalPointerUp();
    });

    this.subSink.sink = this.inputService
      .registerShortcut(new Shortcut({ ctrl: true, key: '+', preventDefault: true, description: 'Zoom In' }))
      .subscribe(() => {
        this.onControlZoomChange(this.graphEditorViewport.scale + 0.1);
        this.viewportScale = this.graphEditorViewport.scale;
      });
    this.subSink.sink = this.inputService
      .registerShortcut(new Shortcut({ ctrl: true, key: '-', preventDefault: true, description: 'Zoom Out' }))
      .subscribe(() => {
        this.onControlZoomChange(this.graphEditorViewport.scale - 0.1);
        this.viewportScale = this.graphEditorViewport.scale;
      });

    this.subSink.sink = this.networkStructureStore.values$.subscribe((structure) => {
      this.personNodes = structure.people;
      this.connectedPersons.clear();
      structure.influenceRelations.forEach((connection) => {
        this.connectedPersons.set(connection, {
          a: this.personNodes.find((p) => p.id === connection.from)!,
          b: this.personNodes.find((p) => p.id === connection.to)!
        });
      });
    });

    this.selection.selected$.subscribe((selectedElements) => {
      this.selectionViewBoxes = selectedElements.map((e) => e.viewBox);
    });
  }

  paneWasInitiallyCentered = false;
  /**
   * Gets called by parent component to center the Viewport.
   */
  onPaneInit(): void {
    this.updateBoundingClientRect();

    if (!this.paneWasInitiallyCentered) {
      // this.graphEditorViewport.position = {
      //   x: -this.graphEditorViewport.width / 2 + this.boundingClientRect.width / 2,
      //   y: -this.graphEditorViewport.height / 2 + this.boundingClientRect.height / 2
      // };
      setTimeout(() => {
        this.enableTransformInterpolation();
      });
      this.paneWasInitiallyCentered = true;
    }
    this.boundsCheckAndConstrain();
    this.applyEditorViewportPositionAndScale();
  }

  enableTransformInterpolation(): void {
    this.graphEditorViewport.transitionInterpolation = true;
  }

  /**
   * Gets called when resizing a pane (and also when resizing the browser window).
   */
  onPaneResize(): void {
    this.updateBoundingClientRect();
    this.boundsCheckAndConstrain();
    this.applyEditorViewportPositionAndScale();
  }

  onGlobalPointerUp(): void {
    this.graphEditorViewport.canBePanned = false;
    this.clickedPerson = undefined;
  }

  lastMousePosition!: { x: number; y: number };

  /**
   * Stores clicked mousePosition and updates bounding rect (not sure if necessary, but is called on low frequency anyway)
   */
  onGraphEditorViewportClick(event: PointerEvent): void {
    if (event.button === 0) this.selection.selectOnly();
    if (event.button !== 1 && event.pointerType != 'touch') return;
    this.graphEditorViewport.canBePanned = true;
    this.lastMousePosition = this.getMousePosition(event);
    this.updateBoundingClientRect();
  }

  onGlobalPointerMove(event: PointerEvent): void {
    const mousePos = this.getMousePosition(event);
    if (
      this.boundingClientRect &&
      mousePos.x >= this.boundingClientRect.left &&
      mousePos.x <= this.boundingClientRect.right &&
      mousePos.y >= this.boundingClientRect.top &&
      mousePos.y <= this.boundingClientRect.bottom
    ) {
      const viewportMousePos = {
        x:
          mousePos.x / this.graphEditorViewport.scale -
          this.graphEditorViewport.position.x -
          this.boundingClientRect.left,
        y:
          mousePos.y / this.graphEditorViewport.scale -
          this.graphEditorViewport.position.y -
          this.boundingClientRect.top
      };
      if (viewportMousePos.x <= this.graphEditorViewport.width && viewportMousePos.y <= this.graphEditorViewport.height)
        this.selection.nextMousePosition(viewportMousePos);
    }
    if (this.graphEditorViewport.canBePanned) this.panGraphEditorViewport(event);
    if (this.clickedPerson !== undefined) {
      const mousePosition = this.getMousePosition(event);

      this.selection.moveNode(this.clickedPerson, {
        x: (mousePosition.x - this.lastMousePosition.x) / this.graphEditorViewport.scale,
        y: (mousePosition.y - this.lastMousePosition.y) / this.graphEditorViewport.scale
      });
      this.lastMousePosition = mousePosition;
    }
  }

  /**
   * The bounding rect of the graphEditor, NOT the graphEditorViewport!
   */
  boundingClientRect!: DOMRect;
  updateBoundingClientRect(): void {
    this.boundingClientRect = this.element.nativeElement.getBoundingClientRect(); // expensive
    // boundingClientRect is relative to the window, and may have to be offset by the scroll position.
    this.boundingClientRect.x += window.scrollX;
    this.boundingClientRect.y += window.scrollY;
  }

  /**
   * @returns the mouse position relative to something, not important, as we only look at the delta.
   */
  getMousePosition(event: PointerEvent | WheelEvent): { x: number; y: number } {
    return {
      x: event.x,
      y: event.y
    };
  }

  @HostListener('wheel', ['$event']) onScroll(event: WheelEvent): void {
    if (event.ctrlKey) {
      event.preventDefault();
    }
  }

  onGraphEditorViewportWheel(event: WheelEvent): void {
    event.preventDefault();
    if (event.ctrlKey) this.zoomGraphEditorViewport(event);
    else {
      if (event.deltaX != 0) this.moveBy({ x: -event.deltaX, y: -event.deltaY });
      else if (event.shiftKey) this.moveBy({ x: -event.deltaY, y: 0 });
      else this.moveBy({ x: 0, y: -event.deltaY });
    }
  }

  panGraphEditorViewport(event: PointerEvent): void {
    const mousePosition = this.getMousePosition(event);
    this.moveBy({
      x: mousePosition.x - this.lastMousePosition.x,
      y: mousePosition.y - this.lastMousePosition.y
    });
    this.lastMousePosition = mousePosition;
  }

  zoomGraphEditorViewport(event: WheelEvent): void {
    const scaleChange = Math.sign(event.deltaY) / 10;
    if (this.graphEditorViewport.scale == this.graphEditorViewport.maxScale) {
      if (this.graphEditorViewport.scale - scaleChange >= this.graphEditorViewport.maxScale) return;
    }
    if (this.graphEditorViewport.scale == this.graphEditorViewport.minScale) {
      if (this.graphEditorViewport.scale - scaleChange <= this.graphEditorViewport.minScale) return;
    }
    this.graphEditorViewport.scale = this.helper.clamp(
      this.graphEditorViewport.scale - scaleChange,
      this.graphEditorViewport.minScale,
      this.graphEditorViewport.maxScale
    );

    const zoomPosition = this.getMousePosition(event);
    const cursorRelativeZoomPosition = {
      x: zoomPosition.x - this.boundingClientRect.x,
      y: zoomPosition.y - this.boundingClientRect.y
    };

    this.moveToPosition({
      x: -event.offsetX + cursorRelativeZoomPosition.x / this.graphEditorViewport.scale,
      y: -event.offsetY + cursorRelativeZoomPosition.y / this.graphEditorViewport.scale
    });
    this.viewportScale = this.graphEditorViewport.scale;
  }

  onControlZoomChange(value: number): void {
    if (!this.paneWasInitiallyCentered) return;
    const scaleChange = this.graphEditorViewport.scale - value;
    this.graphEditorViewport.scale = value;
    const centerZoomPosition = {
      x: this.boundingClientRect.width / 2,
      y: this.boundingClientRect.height / 2
    };

    this.moveToPosition({
      x:
        this.graphEditorViewport.position.x -
        centerZoomPosition.x / (this.graphEditorViewport.scale + scaleChange) +
        centerZoomPosition.x / this.graphEditorViewport.scale,
      y:
        this.graphEditorViewport.position.y -
        centerZoomPosition.y / (this.graphEditorViewport.scale + scaleChange) +
        centerZoomPosition.y / this.graphEditorViewport.scale
    });
  }

  /**
   * Tries to move to the position, automatically performs bounds-check.
   * @param position The position for the Viewport to move to, (0,0) to (-graphEditorViewport.width, -graphEditorViewport.height)
   */
  moveToPosition(position: { x: number; y: number }): void {
    this.graphEditorViewport.position = position;
    this.boundsCheckAndConstrain();
    this.applyEditorViewportPositionAndScale();
    this.graphEditorViewport.centerPosition = this.getCurrentScreenCenterPosition();
    this.selection.graphEditorCenterPosition = this.graphEditorViewport.centerPosition;
  }

  getCurrentScreenCenterPosition(): { x: number; y: number } {
    return {
      x: -this.graphEditorViewport.position.x + this.boundingClientRect.width / 2 / this.graphEditorViewport.scale,
      y: -this.graphEditorViewport.position.y + this.boundingClientRect.height / 2 / this.graphEditorViewport.scale
    };
  }

  moveBy(delta: { x: number; y: number }): void {
    this.moveToPosition({
      x: this.graphEditorViewport.position.x + delta.x / this.graphEditorViewport.scale,
      y: this.graphEditorViewport.position.y + delta.y / this.graphEditorViewport.scale
    });
  }

  tempConnection!: {
    connection: asiaInfluencesRelation;
    personA: asiaPerson;
    personB: asiaPerson;
    cursorPos: { x: number; y: number };
  };
  updateTempConnection(connection: { person: asiaPerson; position: { x: number; y: number } }) {
    this.tempConnection = {
      personA: connection.person,
      personB: { positionX: connection.position.x, positionY: connection.position.y } as asiaPerson,
      connection: new asiaInfluencesRelation('INFLUENCES', Guid.EMPTY, Guid.EMPTY, 0, 0, 0),
      cursorPos: { x: connection.position.x - 40, y: connection.position.y - 40 }
    };
  }
  isCreatingConnection = false;
  setIsCreatingConnectionStatus(status: boolean) {
    this.isCreatingConnection = status;
    this.tempConnection = undefined!;
  }

  clickedPerson!: asiaPerson | undefined;
  onNodeClick(event: PointerEvent, person: asiaPerson) {
    this.lastMousePosition = this.getMousePosition(event);
    this.clickedPerson = person;
  }
  /**
   * Unsubscribe global events
   */
  ngOnDestroy(): void {
    this.subSink.unsubscribe();
  }
}
