import { BehaviorSubject, Observable, shareReplay, switchMap } from 'rxjs';

export class ObservableCache<T> {
  private fetch$ = new BehaviorSubject<void>(undefined);
  public obs$: Observable<T>;

  public refresh(): void {
    this.fetch$.next();
  }
  constructor(obs: Observable<T>) {
    this.obs$ = this.fetch$.pipe(
      switchMap(() => obs),
      shareReplay(1)
    );
  }
}
