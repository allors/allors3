import { IdGenerator } from './database/database-connection';
declare module '@allors/system/workspace/domain' {
  interface Configuration {
    idGenerator: IdGenerator;
  }
}
