import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from 'src/environments/environment';

export enum LoggingLevel {
  DEBUG,
  INFO,
  WARN,
  ERROR,
  OFF
}

@Injectable({
  providedIn: 'root'
})
export class LoggerService {
  private _level: LoggingLevel;
  constructor() {
    this._level = environment.loggingLevel;
  }
  log(level: LoggingLevel, message: string, ...values: any[]) {
    const timestamp = new Date();
    const log = [
      `[${String(timestamp.getHours()).padStart(2, '0')}:${String(timestamp.getMinutes()).padStart(2, '0')}:${String(
        timestamp.getSeconds()
      ).padStart(2, '0')}] ${message}:`,
      ...values
    ];
    if (level != LoggingLevel.OFF && level >= this._level)
      switch (level) {
        case LoggingLevel.DEBUG:
          console.debug(...log);
          break;
        case LoggingLevel.INFO:
          console.info(...log);
          break;
        case LoggingLevel.WARN:
          console.trace('TRACE', log[0]);
          console.warn(...log);
          break;
        case LoggingLevel.ERROR:
          console.trace('TRACE', log[0]);
          console.error(...log);
          break;
        default:
          console.log(...log);
      }
  }
  logDebug(message: string, ...values: any[]): void {
    this.log(LoggingLevel.DEBUG, message, ...values);
  }
  logInfo(message: string, ...values: any[]): void {
    this.log(LoggingLevel.INFO, message, ...values);
  }
  logWarn(message: string, ...values: any[]): void {
    this.log(LoggingLevel.WARN, message, ...values);
  }
  logError(message: string, ...values: any[]): void {
    this.log(LoggingLevel.ERROR, message, ...values);
  }

  rxjsLog<T>(level: LoggingLevel, message: string): (source: Observable<T>) => Observable<T> {
    return (source) =>
      source.pipe(
        tap((val) => {
          this.log(level, message, val);
        })
      );
  }
  rxjsDebug<T>(message: string): (source: Observable<T>) => Observable<T> {
    return (source) =>
      source.pipe(
        tap((val) => {
          this.log(LoggingLevel.DEBUG, message, val);
        })
      );
  }
  rxjsInfo<T>(message: string): (source: Observable<T>) => Observable<T> {
    return (source) =>
      source.pipe(
        tap((val) => {
          this.log(LoggingLevel.INFO, message, val);
        })
      );
  }
  rxjsWarn<T>(message: string): (source: Observable<T>) => Observable<T> {
    return (source) =>
      source.pipe(
        tap((val) => {
          this.log(LoggingLevel.WARN, message, val);
        })
      );
  }
  rxjsError<T>(message: string): (source: Observable<T>) => Observable<T> {
    return (source) =>
      source.pipe(
        tap((val) => {
          this.log(LoggingLevel.ERROR, message, val);
        })
      );
  }
}
