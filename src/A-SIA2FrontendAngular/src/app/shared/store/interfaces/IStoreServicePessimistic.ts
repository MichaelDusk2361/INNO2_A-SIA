import { Guid } from 'guid-typescript';
import { Observable } from 'rxjs';
import { IStoreService } from './IStoreService';

export interface IStoreServicePessimistic<T> extends IStoreService<T> {
  getById(id: Guid): Observable<T>;
  add(value: T): Observable<void>;
  update(value: T): Observable<T>;
  delete(value: T): Observable<void>;
}
