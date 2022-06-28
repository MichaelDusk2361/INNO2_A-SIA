import { asiaGuid } from '../entity.model';

export interface IBaseNode {
  name: string;
  id: asiaGuid;
}

export class DetailsNode<T extends IBaseNode> implements IBaseNode {
  element?: HTMLElement;
  id: asiaGuid;
  url?: 'instances' | 'projects' | 'networks' | 'network';
  data: T;
  name: string;
  children: DetailsNode<IBaseNode>[];
  constructor(data: T, url?: 'instances' | 'projects' | 'networks' | 'network', element?: HTMLElement) {
    this.data = data;
    this.name = data.name;
    this.id = data.id;
    this.children = [];
    if (url !== undefined) this.url = url;
    if (element !== undefined) this.element = element;
  }

  /**
   * copies contents, not children
   * @param node
   * @returns
   */
  static copy(node: DetailsNode<IBaseNode>) {
    return new DetailsNode(node.data, node.url, node.element);
  }
}

export class DetailsNodeHelper {
  private static findRecursiveDFS<T extends IBaseNode>(
    searchId: asiaGuid,
    node: DetailsNode<T>
  ): DetailsNode<T> | undefined {
    if (searchId.toString() === node.id?.toString()) return node;
    for (const child of node.children) {
      const result = this.findRecursiveDFS(searchId, child);
      if (result !== undefined) return result as DetailsNode<T>;
    }
    return undefined;
  }
  private static getPathToRecursive<T extends IBaseNode>(
    searchId: asiaGuid,
    node: DetailsNode<T>,
    path: DetailsNode<T>[]
  ): boolean {
    if (node === undefined) return false;

    if (node.id === searchId) {
      path.push(node);
      return true;
    }

    for (const child of node.children) {
      if (this.getPathToRecursive(searchId, child, path)) {
        path.push(node);
        return true;
      }
    }

    return false;
  }

  static find<T extends IBaseNode>(root: DetailsNode<T>, searchId: asiaGuid): DetailsNode<IBaseNode> | undefined {
    return this.findRecursiveDFS(searchId, root);
  }

  static getPathTo<T extends IBaseNode>(root: DetailsNode<T>, searchId: asiaGuid): DetailsNode<IBaseNode>[] {
    const path: DetailsNode<T>[] = [];
    this.getPathToRecursive(searchId, root, path);
    return path.reverse();
  }

  static getParent<T extends IBaseNode>(root: DetailsNode<T>, searchId: asiaGuid): DetailsNode<IBaseNode> | undefined {
    const path = this.getPathTo(root, searchId);
    if (path.length < 2) return undefined;
    return path[path.length - 2];
  }

  static remove<T extends IBaseNode>(root: DetailsNode<T>, searchId: asiaGuid): DetailsNode<IBaseNode> | undefined {
    const parent = this.getParent(root, searchId);
    const childNodeToRemoveIndex = parent?.children.findIndex((n) => n.id.toString() === searchId.toString());
    if (childNodeToRemoveIndex === undefined) return undefined;
    const childNodeToRemove = parent?.children[childNodeToRemoveIndex];
    parent?.children.splice(childNodeToRemoveIndex, 1);
    return childNodeToRemove as DetailsNode<T>;
  }

  static deepCopy(root: DetailsNode<IBaseNode>): DetailsNode<IBaseNode> {
    const newRoot = DetailsNode.copy(root);
    newRoot.children = [];
    root.children.forEach((node) => {
      newRoot.children.push(this.deepCopy(node));
    });
    return newRoot;
  }
}
