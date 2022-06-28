import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaGroup } from '@shared/models/group.Model';
import { map, Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class GroupService {
  constructor(private http: HttpClient, private logger: LoggerService) {}

  postGroup(networkId: asiaGuid, group: asiaGroup): Observable<void> {
    return this.http
      .post<void>(`${environment.aSiaApiServer}/Group/${networkId}`, group)
      .pipe(this.logger.rxjsInfo('[POST GroupService] Group'));
  }

  putGroup(group: asiaGroup): Observable<asiaGroup> {
    return this.http.put<asiaGroup>(`${environment.aSiaApiServer}/Group/${group.id}`, group).pipe(
      map((res) => asiaGroup.copy(res)),
      this.logger.rxjsInfo('[PUT GroupService] Group')
    );
  }

  deleteGroup(id: asiaGuid): Observable<void> {
    return this.http
      .delete<void>(`${environment.aSiaApiServer}/Group/${id}`)
      .pipe(this.logger.rxjsInfo('[DELETE GroupService] Group'));
  }

  addNodeToGroup(groupId: asiaGuid, nodeId: asiaGuid): Observable<void> {
    return this.http
      .post<void>(`${environment.aSiaApiServer}/Group/${groupId}/${nodeId}`, '')
      .pipe(this.logger.rxjsInfo('[POST GroupService] addNodeToGroup'));
  }

  detachNodeFromGroup(groupId: asiaGuid, nodeId: asiaGuid): Observable<void> {
    return this.http
      .delete<void>(`${environment.aSiaApiServer}/Group/${groupId}/${nodeId}`)
      .pipe(this.logger.rxjsInfo('[DELETE GroupService] detachNodeFromGroup'));
  }

  changeGroupOfNode(groupId: asiaGuid, nodeId: asiaGuid): Observable<void> {
    return this.http
      .patch<void>(`${environment.aSiaApiServer}/Group/${groupId}/${nodeId}`, '')
      .pipe(this.logger.rxjsInfo('[DELETE GroupService] changeGroupOfNode'));
  }
}
