/**
 * Used for command History (undo redo)
 */
export interface ICommand {
  execute: () => void;
  revert: () => void;
}
