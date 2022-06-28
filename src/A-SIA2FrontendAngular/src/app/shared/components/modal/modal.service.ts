import { Injectable } from '@angular/core';
import { ModalComponent } from './modal.component';

@Injectable()
export class ModalService {
  modals: ModalComponent[] = [];
  register(modal: ModalComponent): void {
    this.modals.push(modal);
  }

  public openModal(tag: string): void {
    this.modals.forEach((modal) => {
      if (modal.tag == tag) modal.openModal();
    });
  }

  public closeModal(tag: string): void {
    this.modals.forEach((modal) => {
      if (modal.tag == tag) modal.closeModal();
    });
  }
}
