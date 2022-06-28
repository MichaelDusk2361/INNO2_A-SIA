import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { LoggerService } from '@shared/components/logging/logger.service';
import { asiaGuid } from '@shared/models/entity.model';
import { asiaPerson } from '@shared/models/person.Model';
import { map, Observable, throwError } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PersonService {
  constructor(private http: HttpClient, private logger: LoggerService) {}

  postPerson(networkId: asiaGuid, person: asiaPerson): Observable<void> {
    return this.http
      .post<void>(`${environment.aSiaApiServer}/Person/${networkId}`, {
        ...person,
        simulationValues: Object.fromEntries(person.simulationValues)
      })
      .pipe(this.logger.rxjsInfo('[POST PersonService] Person'));
  }

  putPerson(person: asiaPerson): Observable<asiaPerson> {
    return this.http
      .put<asiaPerson>(`${environment.aSiaApiServer}/Person/${person.id}`, {
        ...person,
        simulationValues: Object.fromEntries(person.simulationValues)
      })
      .pipe(
        map((res) => asiaPerson.copy(res)),
        this.logger.rxjsInfo('[PUT PersonService] Person')
      );
  }

  deletePerson(id: asiaGuid): Observable<void> {
    return this.http
      .delete<void>(`${environment.aSiaApiServer}/Person/${id}`)
      .pipe(this.logger.rxjsInfo('[DELETE PersonService] Person'));
  }
}
