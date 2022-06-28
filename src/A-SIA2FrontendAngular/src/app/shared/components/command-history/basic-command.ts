/* eslint-disable @typescript-eslint/no-explicit-any */
import { ICommand } from './command';

/**
 * Basic ICommand implementation (similar to RelayCommand in WPF), takes two lambdas and calls them upon
 * execute / revert. Be careful about pass by value / reference when capturing variables from outside.
 * If there is something going wrong its probably that. You can alternatively derive from ICommand yourself.
 *
 * @example
 *
 * ```ts
 * const exampleMoveCommand = new ExampleMoveCommand(this.node, this.node.position, {
 *    x: this.node.position.x + Math.floor(Math.random() * 10),
 *    y: this.node.position.y + Math.floor(Math.random() * 10)
 *  });
 *  const oldPosition = this.node.position;
 *  const newPosition = {
 *    x: this.node.position.x + Math.floor(Math.random() * 10),
 *    y: this.node.position.y + Math.floor(Math.random() * 10)
 *  };
 *  const exampleMoveCommand2 = new BasicCommand(
 *    (): void => {
 *      this.node.setPosition(newPosition);
 *      console.log('node is now at ' + this.node.position.x + ', ' + this.node.position.y);
 *    },
 *    (): void => {
 *      this.node.setPosition(oldPosition);
 *      console.log('node is now at ' + this.node.position.x + ', ' + this.node.position.y);
 *    }
 *  );
 *  exampleMoveCommand2.execute();
 *  this.commandHistoryService.append(exampleMoveCommand2);
 * ```
 */
export class BasicCommand implements ICommand {
  execute(): void {
    this.executeCallback();
  }
  revert(): void {
    this.undoCallback();
  }

  executeCallback!: (...args: any[]) => void;
  undoCallback!: (...args: any[]) => void;

  constructor(executeCallback: (...args: any[]) => void, undoCallback: (...args: any[]) => void) {
    this.executeCallback = executeCallback;
    this.undoCallback = undoCallback;
  }
}
