import { catchError, Observable, of, throwError } from 'rxjs';

/**
 * @param errors error messages that will be ignored
 * @returns either an observable that publishes undefined, which calls next and then completes,
 * or an Observable that rethrows,
 * or the source if nothing was thrown beforehand.
 */
export function ignoreErrors<T>(...errors: string[]): (source: Observable<T>) => Observable<T | undefined> {
  return (source) =>
    source.pipe(
      catchError((err) => {
        if (errors.includes(err)) return of(undefined);
        return throwError(() => new Error(err).message);
      })
    );
}
