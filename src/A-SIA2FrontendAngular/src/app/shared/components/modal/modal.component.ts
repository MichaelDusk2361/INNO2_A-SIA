import { Component, ElementRef, HostBinding, Input } from '@angular/core';
import { InputService } from '@shared/components/user-input/input.service';
import { ModalService } from './modal.service';

@Component({
  selector: 'a-sia-modal',
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.scss']
})
export class ModalComponent {
  @Input() tag = 'modalName';
  @Input() @HostBinding('class.open') isOpen = false;

  constructor(private inputService: InputService, private el: ElementRef, private modalService: ModalService) {
    this.inputService.onPointerDown$.subscribe((event) => this.closeWhenModalNotClicked(event));
    this.modalService.register(this);
  }

  closeWhenModalNotClicked(event: MouseEvent): void {
    if (!this.el.nativeElement.contains(event.target)) {
      this.closeModal();
    }
  }

  openModal(): void {
    this.isOpen = true;
  }

  closeModal(): void {
    this.isOpen = false;
  }
}
