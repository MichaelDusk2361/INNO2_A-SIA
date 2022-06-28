import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaInfluencesRelation } from '@shared/models/relations/influencesRelation.model';
import { map, Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class RelationService {
  constructor(private http: HttpClient, private logger: LoggerService) {}

  postInfluencesRelation(connection: asiaInfluencesRelation): Observable<void> {
    return this.http
      .post<void>(`${environment.aSiaApiServer}/Relation/${connection.from}/Influences/${connection.to}`, connection)
      .pipe(this.logger.rxjsInfo('[POST RelationService] Connection'));
  }

  putInfluencesRelation(connection: asiaInfluencesRelation): Observable<asiaInfluencesRelation> {
    return this.http
      .put<asiaInfluencesRelation>(`${environment.aSiaApiServer}/Relation/Influences/${connection.id}`, connection)
      .pipe(
        map((res) => asiaInfluencesRelation.copy(res)),
        this.logger.rxjsInfo('[PUT RelationService] Connection')
      );
  }

  deleteInfluencesRelation(id: asiaGuid): Observable<void> {
    return this.http
      .delete<void>(`${environment.aSiaApiServer}/Relation/Influences/${id}`)
      .pipe(this.logger.rxjsInfo('[DELETE RelationService] Connection'));
  }
}
