// This file can be replaced during build by using the `fileReplacements` array.
// `ng build` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

// importing should work perfectly fine, and it did, but it suddenly stopped working and doesn't know LoggingLevel on runtime. Oh well.
// import { LoggingLevel } from '@shared/components/logging/logger.service';
enum LoggingLevel {
  DEBUG,
  INFO,
  WARN,
  ERROR,
  OFF
}
export const environment = {
  production: false,
  aSiaApiServer: 'http://localhost:5000/api',
  loggingLevel: LoggingLevel.DEBUG
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/plugins/zone-error';  // Included with Angular CLI.
