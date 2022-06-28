import { IStoreService } from './IStoreService';

export interface IStoreServiceOptimistic<T> extends IStoreService<T> {
  add(value: T): void;
  update(value: T): void;
  delete(value: T): void;
}
