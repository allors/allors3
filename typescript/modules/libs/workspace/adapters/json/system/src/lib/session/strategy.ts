import { Session, Strategy as SystemStrategy } from '@allors/workspace/adapters/system';
import { Class, Origin } from '@allors/workspace/meta/system';
import { DatabaseOriginState } from './originstate/database-origin-state';
import { DatabaseRecord } from '../database/database-record';

export class Strategy extends SystemStrategy {
  constructor(public session: Session, public cls: Class, public id: number) {
    super(session, cls, id);

    if (this.cls.origin === Origin.Database) {
      this.DatabaseOriginState = new DatabaseOriginState(this, session.workspace.database.getRecord(this.id));
    }
  }

  static fromDatabaseRecord(session: Session, databaseRecord: DatabaseRecord) {
    return new Strategy(session, databaseRecord.cls, databaseRecord.id);
  }
}
