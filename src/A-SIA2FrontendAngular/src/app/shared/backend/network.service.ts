import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { delay, map, Observable, share, throwError } from 'rxjs';
import { environment } from 'src/environments/environment';
import { asiaNetwork } from '../models/network.model';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';

@Injectable({
  providedIn: 'root'
})
export class NetworkService {
  constructor(private http: HttpClient, private logger: LoggerService) {}

  postNetwork(network: asiaNetwork): Observable<void> {
    return this.http
      .post<void>(`${environment.aSiaApiServer}/Network`, network)
      .pipe(this.logger.rxjsInfo('[POST] Network'));
  }

  getNetwork(id: asiaGuid): Observable<asiaNetwork> {
    return this.http.get<asiaNetwork>(`${environment.aSiaApiServer}/Network/${id}`).pipe(
      map((res) => asiaNetwork.copy(res)),
      this.logger.rxjsInfo('[GET] Network'),
      share()
    );
  }

  putNetwork(Network: asiaNetwork): Observable<asiaNetwork> {
    return this.http.put<asiaNetwork>(`${environment.aSiaApiServer}/Network/${Network.id}`, Network).pipe(
      map((res) => asiaNetwork.copy(res)),
      this.logger.rxjsInfo('[PUT] Network')
    );
  }

  deleteNetwork(id: asiaGuid): Observable<void> {
    return this.http
      .delete<void>(`${environment.aSiaApiServer}/Network/${id}`)
      .pipe(this.logger.rxjsInfo('[DELETE] Network'));
  }
}
