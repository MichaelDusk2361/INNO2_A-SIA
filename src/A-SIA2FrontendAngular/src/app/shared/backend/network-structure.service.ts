import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaNetworkStructure } from '@shared/models/network-structure.model';
import { Observable, share, map } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class NetworkStructureService {
  constructor(private http: HttpClient, private logger: LoggerService) {}

  private readonly _getNetworkStructureObservables: Map<asiaGuid, Observable<asiaNetworkStructure>> = new Map();

  getNetworkStructure(networkId: asiaGuid): Observable<asiaNetworkStructure> {
    const obs = this._getNetworkStructureObservables.get(networkId);
    if (obs !== undefined) return obs;

    const newObs = this.http
      .get<asiaNetworkStructure>(`${environment.aSiaApiServer}/NetworkStructure/${networkId}`)
      .pipe(
        map((res) => asiaNetworkStructure.copy(res)),
        this.logger.rxjsInfo('[GET NetworkStructureService] NetworkStructure ' + networkId),
        share()
      );
    this._getNetworkStructureObservables.set(networkId, newObs);
    return newObs;
  }
}
