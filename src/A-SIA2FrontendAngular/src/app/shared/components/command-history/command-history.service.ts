import { Injectable } from '@angular/core';
import { InputService } from '@shared/components/user-input/input.service';
import { Shortcut } from '@shared/components/user-input/shortcut';
import { ICommand } from './command';

/**
 * Provides a command history, to use just inject in constructor.
 * With the function append, you can add a command in the history. Make sure to execute it beforehand.
 * The functions undo and redo can be used to navigate in the command history.
 * If append is called while you are a number of steps into the history,
 * all commands that would be executed by redoing, will be removed from the history.
 * The maxHistorySize can be set, and will limit the capacity of the History, removing the oldest entry if exceeded.
 *
 */
@Injectable({
  providedIn: 'root'
})
export class CommandHistoryService {
  private commandHistory: ICommand[] = [];
  private maxHistorySize = 50;
  private stepsBackInHistory = 0;

  undoShortcut = new Shortcut({ key: 'z', ctrl: true, description: 'undo last action' });
  redoShortcutY = new Shortcut({ key: 'y', ctrl: true, description: 'redo last action' });
  redoShortcutZ = new Shortcut({ key: 'z', shift: true, ctrl: true, description: 'redo last action' });

  constructor(private inputService: InputService) {
    inputService.registerShortcut(this.undoShortcut).subscribe(() => {
      this.undo();
    });
    inputService.registerShortcut(this.redoShortcutY).subscribe(() => {
      this.redo();
    });
    inputService.registerShortcut(this.redoShortcutZ).subscribe(() => {
      this.redo();
    });
  }

  undo = (): void => {
    if (!this.canUndo) return;
    this.stepsBackInHistory++;
    this.commandHistory[this.commandHistory.length - this.stepsBackInHistory].revert();
  };

  get canRedo(): boolean {
    return this.stepsBackInHistory != 0 && this.commandHistory.length != 0;
  }
  get canUndo(): boolean {
    return this.stepsBackInHistory != this.commandHistory.length && this.commandHistory.length != 0;
  }

  redo = (): void => {
    if (!this.canRedo) return;
    this.commandHistory[this.commandHistory.length - this.stepsBackInHistory].execute();
    this.stepsBackInHistory--;
  };

  append(command: ICommand): void {
    if (this.stepsBackInHistory != 0) this.commandHistory.splice(-this.stepsBackInHistory);
    this.stepsBackInHistory = 0;
    this.commandHistory.push(command);
    if (this.commandHistory.length > this.maxHistorySize) this.commandHistory.shift();
  }
}
