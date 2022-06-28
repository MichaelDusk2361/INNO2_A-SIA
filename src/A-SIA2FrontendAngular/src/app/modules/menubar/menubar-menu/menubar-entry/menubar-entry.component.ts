import { Component, HostBinding, HostListener, Input } from '@angular/core';
import { MenubarService } from '../../menubar.service';
/**
 * A single entry, which calls an assigned function on click
 */
@Component({
  selector: 'a-sia-menubar-entry',
  templateUrl: './menubar-entry.component.html',
  styleUrls: ['./menubar-entry.component.scss']
})
export class MenubarEntryComponent {
  constructor(private menubarService: MenubarService) {}

  /**
   * The function to call onclick
   * Only pass arrow functions, not the function itself!
   * If you pass a regular function, the this context will be the MenubarEntry instead of the class that owns the function
   */
  @Input() action = (): void => {
    alert('non implemented menubar entry');
  };
  @Input() @HostBinding('class.disabled') disabled = false;
  @Input() closeOnAction = true;

  @Input() shortcutText!: string;
  /**
   * Invokes action onclick
   */
  @HostListener('click')
  onClick(): void {
    if (this.disabled) return;
    this.action();
    if (this.closeOnAction) this.menubarService.closeAll();
  }
}
