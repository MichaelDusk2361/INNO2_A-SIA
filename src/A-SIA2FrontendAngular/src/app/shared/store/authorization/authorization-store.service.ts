import { Injectable } from '@angular/core';
import { AuthorizationService } from '@shared/backend/authorization.service';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaUser } from '@shared/models/user.model';
import { BehaviorSubject } from 'rxjs';

/**
 * For comments, see {@link InstanceStoreService}, which follows the same structure.
 */
@Injectable({
  providedIn: 'root'
})
export class AuthorizationStoreService {
  private readonly _userSource = new BehaviorSubject<asiaUser | null>(null);
  get user$() {
    return this._userSource.asObservable().pipe(this.logger.rxjsDebug('[AuthorizationStoreService] get user$'));
  }

  constructor(private authorizationService: AuthorizationService, private logger: LoggerService) {
    this.load();
  }

  private load(): void {
    this.authorizationService.getLoggedInUser$.subscribe((res) => this._userSource.next(res));
  }
}
