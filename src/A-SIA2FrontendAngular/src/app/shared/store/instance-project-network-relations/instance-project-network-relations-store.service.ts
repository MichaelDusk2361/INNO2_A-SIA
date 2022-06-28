import { Injectable } from '@angular/core';
import { UserService } from '@shared/backend/user.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaInstanceProjectNetworkRelations } from '@shared/models/instanceProjectNetworkRelations.model';
import { BehaviorSubject, Observable } from 'rxjs';

/**
 * For comments, see {@link InstanceStoreService}, which follows the same structure.
 */
@Injectable({
  providedIn: 'root'
})
export class InstanceProjectNetworkRelationsStoreService {
  _source = new BehaviorSubject<asiaInstanceProjectNetworkRelations>(new asiaInstanceProjectNetworkRelations([], []));
  get values$(): Observable<asiaInstanceProjectNetworkRelations> {
    return this._source
      .asObservable()
      .pipe(this.logger.rxjsDebug('[InstanceProjectNetworkRelationsStoreService] get values$'));
  }

  constructor(private logger: LoggerService, private userService: UserService) {
    this._load();
  }
  _load(): void {
    this.userService.getInstanceProjectNetworkRelations$.subscribe((res) => this._source.next(res));
  }
  _getSourceValues(): asiaInstanceProjectNetworkRelations {
    return this._source.getValue();
  }
  _setSourceValues(value: asiaInstanceProjectNetworkRelations) {
    this._source.next(value);
  }
}
