import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaProject } from '@shared/models/project.model';
import { map, Observable, share } from 'rxjs';
import { environment } from 'src/environments/environment';
import { asiaNetwork } from '../models/network.model';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  constructor(private http: HttpClient, private logger: LoggerService) {}

  postProject(project: asiaProject): Observable<void> {
    return this.http
      .post<void>(`${environment.aSiaApiServer}/Project`, project)
      .pipe(this.logger.rxjsInfo('[POST ProjectService] Project'));
  }

  getProject(id: asiaGuid): Observable<asiaProject> {
    return this.http.get<asiaProject>(`${environment.aSiaApiServer}/Project/${id}`).pipe(
      map((res) => asiaProject.copy(res)),
      this.logger.rxjsInfo('[GET ProjectService] Projects')
    );
  }

  postProjectNetwork(id: asiaGuid, network: asiaNetwork): Observable<asiaNetwork> {
    return this.http.post<asiaNetwork>(`${environment.aSiaApiServer}/Project/${id}/Network`, network).pipe(
      map((res) => asiaNetwork.copy(res)),
      this.logger.rxjsInfo('[POST ProjectService] ProjectNetwork')
    );
  }

  putProject(project: asiaProject): Observable<asiaProject> {
    return this.http.put<asiaProject>(`${environment.aSiaApiServer}/Project/${project.id}`, project).pipe(
      map((res) => asiaProject.copy(res)),
      this.logger.rxjsInfo('[PUT ProjectService] Project')
    );
  }

  deleteProject(id: asiaGuid): Observable<void> {
    return this.http
      .delete<void>(`${environment.aSiaApiServer}/Project/${id}`)
      .pipe(this.logger.rxjsInfo('[DELETE ProjectService] Project'));
  }
}
