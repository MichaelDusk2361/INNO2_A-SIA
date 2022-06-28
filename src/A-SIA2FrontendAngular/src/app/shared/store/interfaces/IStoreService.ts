import { BehaviorSubject, Observable } from 'rxjs';

export interface IStoreService<T> {
  readonly _source: BehaviorSubject<T[]>;
  readonly values$: Observable<T[]>;
  _load(): void;
  _getSourceValues(): T[];
  _setSourceValues(value: T[]): void;
}
