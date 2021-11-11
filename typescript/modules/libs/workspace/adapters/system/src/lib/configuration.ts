import { IdGenerator } from './database/database-connection';
declare module '@allors/workspace/domain/system' {
  interface Configuration {
    idGenerator: IdGenerator;
  }
}
