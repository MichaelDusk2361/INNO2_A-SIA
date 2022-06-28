import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MenubarComponent } from './menubar.component';
import { MenubarMenuComponent } from './menubar-menu/menubar-menu.component';
import { MenubarEntryComponent } from './menubar-menu/menubar-entry/menubar-entry.component';
import { SharedModule } from '@shared/shared.module';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { AppRoutingModule } from 'src/app/app-routing.module';

/**
 * See MenubarComponent for usage
 */
@NgModule({
  declarations: [MenubarComponent, MenubarMenuComponent, MenubarEntryComponent],
  imports: [CommonModule, SharedModule, FontAwesomeModule, AppRoutingModule],
  exports: [MenubarComponent]
})
export class MenubarModule {}
