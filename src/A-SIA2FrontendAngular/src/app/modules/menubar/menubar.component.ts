import { Component, ElementRef, QueryList, ViewChildren } from '@angular/core';
import { faRedo, faUndo } from '@fortawesome/free-solid-svg-icons';
import { BasicCommand } from '@shared/components/command-history/basic-command';
import { CommandHistoryService } from '@shared/components/command-history/command-history.service';
import { ExampleMoveCommand } from '@shared/components/command-history/example-command';
import { ExampleNode } from '@shared/components/command-history/example-node';
import { PaneService } from 'src/app/pane.service';
import { InputService } from '@shared/components/user-input/input.service';
import { ModalService } from '@shared/components/modal/modal.service';
import { MenubarMenuComponent } from './menubar-menu/menubar-menu.component';
import { MenubarService } from './menubar.service';
import { ProjectHierarchyStoreService } from '@shared/store/project-hierarchy/project-hierarchy-store.service';

/**
 * Top level component for the menubar.
 * it contains menubar-menus (file, view), which in turn contain menubar-entries (save, open).
 * @example
 * ```html
 * <!-- Text within components html selectors will replace their respective <ng-Content> -->
 * <a-sia-menubar-menu label="LABEL NAME 1">
 *   <a-sia-menubar-entry [action]="ONCLICK CALLBACK FOR ENTRY">ENTRY NAME 1</a-sia-menubar-entry>
 *   <a-sia-menubar-entry [action]="ONCLICK CALLBACK FOR ENTRY">ENTRY NAME 2</a-sia-menubar-entry>
 * </a-sia-menubar-menu>
 * <a-sia-menubar-menu label="LABEL NAME 2">
 *   <a-sia-menubar-entry [action]="ONCLICK CALLBACK FOR ENTRY">ENTRY NAME 1</a-sia-menubar-entry>
 * </a-sia-menubar-menu>
 * ```
 */
@Component({
  selector: 'a-sia-menubar',
  templateUrl: './menubar.component.html',
  styleUrls: ['./menubar.component.scss'],
  providers: [MenubarService]
})
export class MenubarComponent {
  undoIcon = faUndo;
  redoIcon = faRedo;
  node: ExampleNode = new ExampleNode();
  constructor(
    private inputService: InputService,
    private menubarService: MenubarService,
    public paneService: PaneService,
    public modalService: ModalService,
    public commandHistoryService: CommandHistoryService,
    private hierarchy: ProjectHierarchyStoreService
  ) {
    this.inputService.onPointerDown$.subscribe((event) => this.closeWhenMenubarNotClicked(event));
  }
  /**
   * Get all MenubarMenus.
   * {read: ElementRef} is needed to get the Element instead of the Component
   */
  @ViewChildren(MenubarMenuComponent, { read: ElementRef }) menubarMenus!: QueryList<ElementRef>;

  public menubarExampleCallback(): void {
    console.log('example menubar callback was invoked');
  }

  public runExampleMoveCommand = (): void => {
    const exampleMoveCommand = new ExampleMoveCommand(this.node, this.node.position, {
      x: this.node.position.x + Math.floor(Math.random() * 10),
      y: this.node.position.y + Math.floor(Math.random() * 10)
    });
    const oldPosition = this.node.position;
    const newPosition = {
      x: this.node.position.x + Math.floor(Math.random() * 10),
      y: this.node.position.y + Math.floor(Math.random() * 10)
    };
    const exampleMoveCommand2 = new BasicCommand(
      (): void => {
        this.node.setPosition(newPosition);
        console.log('node is now at ' + this.node.position.x + ', ' + this.node.position.y);
      },

      (): void => {
        this.node.setPosition(oldPosition);
        console.log('node is now at ' + this.node.position.x + ', ' + this.node.position.y);
      }
    );
    exampleMoveCommand2.execute();
    this.commandHistoryService.append(exampleMoveCommand2);
  };

  /**
   * checks if some of the menus contain the event.target, if not it closes all menus
   */
  closeWhenMenubarNotClicked(event: PointerEvent): void {
    if (!this.menubarMenus.some((menu) => menu.nativeElement.contains(event.target))) {
      this.menubarService.closeAll();
    }
  }

  openLoginModal = (): void => {
    this.modalService.openModal('login');
  };

  openSettingsModal = (): void => {
    this.modalService.openModal('settings');
  };
}
