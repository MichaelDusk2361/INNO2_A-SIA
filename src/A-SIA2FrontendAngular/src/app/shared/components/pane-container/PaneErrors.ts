/**
 * Error for everything related to a Pane.
 * There are different Errors for pane, container, and divider, mainly for easily noticing where the error originated.
 */
export class PaneError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'PaneError';
  }
}
/**
 * Error for everything related to a Pane Divider
 * There are different Errors for pane, container, and divider, mainly for easily noticing where the error originated.
 */
export class PaneDividerError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'PaneDividerError';
  }
}
/**
 * Error for everything related to the container
 * There are different Errors for pane, container, and divider, mainly for easily noticing where the error originated.
 */
export class PaneContainerError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'PaneContainerError';
  }
}
