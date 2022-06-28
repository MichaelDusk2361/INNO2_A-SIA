import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { asiaInstance } from '../models/instance.model';
import { asiaNetwork } from '../models/network.model';
import { asiaProject } from '../models/project.model';
import { map, Observable, share } from 'rxjs';
import { asiaInstanceProjectNetworkRelations } from '../models/instanceProjectNetworkRelations.model';
import { LoggerService } from '@shared/components/logging/logger.service';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  constructor(private http: HttpClient, private logger: LoggerService) {}

  // We are mapping the result of http requests, so that it will be created with the right constructor.
  // Without doing that, we couldn't use instanceof

  // [warm] gets all instances of the logged in user
  readonly getInstances$: Observable<asiaInstance[]> = this.http
    .get<asiaInstance[]>(`${environment.aSiaApiServer}/User/instances`)
    .pipe(
      map((res) => res.map((i) => asiaInstance.copy(i))),
      this.logger.rxjsInfo('[GET UserService] Instances'),
      share()
    );
  // [warm] gets all projects of the logged in user
  readonly getProjects$: Observable<asiaProject[]> = this.http
    .get<asiaProject[]>(`${environment.aSiaApiServer}/User/projects`)
    .pipe(
      map((res) => res.map((p) => asiaProject.copy(p))),
      this.logger.rxjsInfo('[GET UserService] Projects'),
      share()
    );
  // [warm] gets all networks of the logged in user
  readonly getNetworks$: Observable<asiaNetwork[]> = this.http
    .get<asiaNetwork[]>(`${environment.aSiaApiServer}/User/networks`)
    .pipe(
      map((res) => res.map((n) => asiaNetwork.copy(n))),
      this.logger.rxjsInfo('[GET UserService] Networks'),
      share()
    );
  // [warm] gets all project->network and instance->project relations of the logged in user
  readonly getInstanceProjectNetworkRelations$: Observable<asiaInstanceProjectNetworkRelations> = this.http
    .get<asiaInstanceProjectNetworkRelations>(`${environment.aSiaApiServer}/User/instanceProjectNetworkRelations`)
    .pipe(
      map((res) => asiaInstanceProjectNetworkRelations.copy(res)),
      this.logger.rxjsInfo('[GET UserService] InstanceProjectNetworkRelations'),
      share()
    );
}
