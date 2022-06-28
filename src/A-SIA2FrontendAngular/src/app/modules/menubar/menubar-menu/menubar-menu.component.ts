import { Component, HostBinding, Input } from '@angular/core';
import { MenubarService } from '../menubar.service';

/**
 * A Menubar menu, which contains multiple entries.
 * The opening and closing of the menu is implemented on hover of the menubar-menu component.
 *
 * For example, ng-content will be replaced with:
 * ```html
 *   <a-sia-menubar-entry [action]="ONCLICK CALLBACK FOR ENTRY">ENTRY NAME 1</a-sia-menubar-entry>
 * ```
 */
@Component({
  selector: 'a-sia-menubar-menu',
  templateUrl: './menubar-menu.component.html',
  styleUrls: ['./menubar-menu.component.scss']
})
export class MenubarMenuComponent {
  /**
   * The text to be displayed in the main menubar. e.g. File, View, Edit
   */
  @Input() label = 'Menu';

  /**
   * tracks current open / close status, used for toggle
   */
  @HostBinding('class.open') isOpen = false;

  /**
   * adds this menu to the list of all menu, i.e. to be able to close all menus
   * @param menubarService service overseeing the whole menubar
   */
  constructor(private menubarService: MenubarService) {
    this.menubarService.addMenu(this);
  }

  /**
   * For Mobile: this is invoked before onEnter methods, and therefore used to set a bool that
   * guards the onEnter Methods from being processed.
   */
  isTouchEvent = false;
  onTouchStart(): void {
    this.isTouchEvent = true;
  }

  /**
   * Opens the menu on mouseEnter if the menubar was clicked. The isTouchEvent prevents wrong behavior on mobile.
   * (on mobile, both onLabelMouseEnter and onLabelClick would be called with one click, opening and closing the menu immediately.)
   */
  onLabelMouseEnter(): void {
    if (this.isTouchEvent) {
      return;
    }
    if (this.menubarService.menuWasClicked) this.openMenu();
  }

  /**
   * Opens the menu
   */
  onLabelClick(): void {
    this.toggleMenu();
    this.menubarService.menuWasClicked = !this.menubarService.menuWasClicked;
  }

  /**
   * calls closeMenu() or openMenu()
   */
  toggleMenu(): void {
    if (this.isOpen) this.closeMenu();
    else this.openMenu();
  }

  openMenu(): void {
    this.menubarService.closeMenusExcept(this);
    this.isOpen = true;
  }

  /**
   * The isTouchEvent reset to false is probably rarely necessary, except if a device is both touch and mouse capable,
   * and the user switches between them. But just in case, it is being reset to be ready for the next interaction.
   */
  closeMenu(): void {
    this.isOpen = false;
    this.isTouchEvent = false;
  }
}
