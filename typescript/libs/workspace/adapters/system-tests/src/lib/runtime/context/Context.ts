import { IObject, ISession, IWorkspace } from '@allors/workspace/domain/system';
import { Class, Origin } from '@allors/workspace/meta/system';
import { Fixture } from '../../Fixture';
import { DatabaseMode } from './modes/DatabaseMode';
import { WorkspaceMode } from './modes/WorkspaceMode';

export abstract class Context {
  constructor(public fixture: Fixture, public name: string) {
    this.sharedDatabaseWorkspace = this.fixture.createWorkspace();
    this.sharedDatabaseSession = this.sharedDatabaseWorkspace.createSession();
    this.exclusiveDatabaseWorkspace = this.fixture.createExclusiveWorkspace();
    this.exclusiveDatabaseSession = this.exclusiveDatabaseWorkspace.createSession();
  }

  session1: ISession;

  session2: ISession;

  sharedDatabaseWorkspace: IWorkspace;

  sharedDatabaseSession: ISession;

  exclusiveDatabaseWorkspace: IWorkspace;

  exclusiveDatabaseSession: ISession;

  get asyncClient() {
    return this.fixture.asyncClient;
  }

  async create<T extends IObject>(session: ISession, cls: Class, mode: DatabaseMode): Promise<T>;
  async create<T extends IObject>(session: ISession, cls: Class, mode: WorkspaceMode): Promise<T>;
  async create<T extends IObject>(session: ISession, cls: Class, mode: DatabaseMode | WorkspaceMode): Promise<T> {
    if (cls.origin === Origin.Database) {
      switch (mode as DatabaseMode) {
        case DatabaseMode.NoPush:
          return session.create<T>(cls);
        case DatabaseMode.Push: {
          const pushObject = session.create<T>(cls);
          await this.asyncClient.pushAsync(session);
          return pushObject;
        }
        case DatabaseMode.PushAndPull: {
          const pushAndPullObject = session.create<T>(cls);
          const result = await this.asyncClient.pushAsync(session);
          if (result.hasErrors) throw new Error();
          await this.asyncClient.pullAsync(session, { object: pushAndPullObject });
          return pushAndPullObject;
        }
        case DatabaseMode.SharedDatabase: {
          const sharedDatabaseObject = this.sharedDatabaseSession.create<T>(cls);
          await this.asyncClient.pushAsync(this.sharedDatabaseSession);
          await this.asyncClient.pullAsync(session, { object: sharedDatabaseObject });
          return sharedDatabaseObject;
        }
        case DatabaseMode.ExclusiveDatabase: {
          const exclusiveDatabaseObject = this.exclusiveDatabaseSession.create<T>(cls);
          await this.asyncClient.pushAsync(this.exclusiveDatabaseSession);
          await this.asyncClient.pullAsync(session, { object: exclusiveDatabaseObject });
          return exclusiveDatabaseObject;
        }
        default:
          throw new Error(mode.toString());
      }
    } else {
      switch (mode as WorkspaceMode) {
        case WorkspaceMode.NoPush:
          return session.create<T>(cls);
        case WorkspaceMode.Push: {
          const pushObject = session.create<T>(cls);
          session.pushToWorkspace();
          return pushObject;
        }
        case WorkspaceMode.PushAndPull: {
          const pushAndPullObject = session.create<T>(cls);
          session.pushToWorkspace();
          session.pullFromWorkspace();
          return pushAndPullObject;
        }
        default:
          throw new Error(mode.toString());
      }
    }
  }
}
