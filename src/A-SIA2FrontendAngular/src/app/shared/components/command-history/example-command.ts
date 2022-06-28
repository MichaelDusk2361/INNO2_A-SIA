import { ICommand } from './command';
import { ExampleNode, IPosition } from './example-node';

/**
 * Example implementation of what a simple move command could look like.
 * The constructor is requiring all data to execute / revert this command.
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
export class ExampleMoveCommand implements ICommand {
  oldPosition!: IPosition;
  newPosition!: IPosition;
  nodeToMove!: ExampleNode;
  execute(): void {
    this.nodeToMove.setPosition(this.newPosition);
    console.log('node is now at ' + this.nodeToMove.position.x + ', ' + this.nodeToMove.position.y);
  }

  revert(): void {
    this.nodeToMove.setPosition(this.oldPosition);
    console.log('node is now at ' + this.nodeToMove.position.x + ', ' + this.nodeToMove.position.y);
  }
  constructor(nodeToMove: ExampleNode, oldPosition: IPosition, newPosition: IPosition) {
    this.oldPosition = oldPosition;
    this.newPosition = newPosition;
    this.nodeToMove = nodeToMove;
  }
}
