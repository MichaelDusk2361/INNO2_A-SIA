import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { asiaInstance } from '../models/instance.model';
import { asiaProject } from '../models/project.model';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';

@Injectable({
  providedIn: 'root'
})
export class InstanceService {
  constructor(private http: HttpClient, private logger: LoggerService) {}

  postInstance(instance: asiaInstance): Observable<void> {
    return this.http
      .post<void>(`${environment.aSiaApiServer}/Instance`, instance)
      .pipe(this.logger.rxjsInfo('[POST InstanceService] Instance'));
  }

  getInstance(id: asiaGuid): Observable<asiaInstance> {
    return this.http.get<asiaInstance>(`${environment.aSiaApiServer}/Instance/${id}`).pipe(
      map((res) => asiaInstance.copy(res)),
      this.logger.rxjsInfo('[GET InstanceService] Instances')
    );
  }

  postInstanceProject(id: asiaGuid, project: asiaProject): Observable<asiaProject> {
    return this.http.post<asiaProject>(`${environment.aSiaApiServer}/Instance/${id}/Project`, project).pipe(
      map((res) => asiaProject.copy(res)),
      this.logger.rxjsInfo('[POST InstanceService] InstanceProject')
    );
  }

  putInstance(instance: asiaInstance): Observable<asiaInstance> {
    return this.http.put<asiaInstance>(`${environment.aSiaApiServer}/Instance/${instance.id}`, instance).pipe(
      map((res) => asiaInstance.copy(res)),
      this.logger.rxjsInfo('[PUT InstanceService] Instance')
    );
  }

  deleteInstance(id: asiaGuid): Observable<void> {
    return this.http
      .delete<void>(`${environment.aSiaApiServer}/Instance/${id}`)
      .pipe(this.logger.rxjsInfo('[DELETE InstanceService] Instance'));
  }
}
