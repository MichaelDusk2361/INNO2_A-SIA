enum LoggingLevel {
  DEBUG,
  INFO,
  WARN,
  ERROR,
  OFF
}
export const environment = {
  production: true,
  aSiaApiServer: 'https://a-sia.miduskanich.com/api',
  loggingLevel: LoggingLevel.DEBUG
};
