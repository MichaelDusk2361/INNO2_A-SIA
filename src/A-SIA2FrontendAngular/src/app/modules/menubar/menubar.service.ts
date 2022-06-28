import { Injectable } from '@angular/core';
import { MenubarMenuComponent } from './menubar-menu/menubar-menu.component';

@Injectable()
export class MenubarService {
  menubarMenus: MenubarMenuComponent[] = [];
  public menuWasClicked = false;

  public closeMenusExcept(menuToKeepOpen: MenubarMenuComponent): void {
    this.menubarMenus.forEach((menu) => {
      if (menu != menuToKeepOpen) menu.closeMenu();
    });
  }

  public closeAll(): void {
    this.menubarMenus.forEach((menu) => {
      menu.closeMenu();
    });
    this.menuWasClicked = false;
  }

  public addMenu(menu: MenubarMenuComponent): void {
    this.menubarMenus.push(menu);
  }
}
