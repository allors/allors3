import { IObject, ISession, IWorkspace } from '@allors/system/workspace/domain';
import { Class, Origin } from '@allors/system/workspace/meta';
import { Fixture } from '../../fixture';
import { DatabaseMode } from './modes/database-mode';

export abstract class Context {
  constructor(public fixture: Fixture, public name: string) {
    this.sharedDatabaseWorkspace = this.fixture.createWorkspace();
    this.sharedDatabaseSession = this.sharedDatabaseWorkspace.createSession();
    this.exclusiveDatabaseWorkspace = this.fixture.createExclusiveWorkspace();
    this.exclusiveDatabaseSession =
      this.exclusiveDatabaseWorkspace.createSession();
  }

  session1: ISession;

  session2: ISession;

  sharedDatabaseWorkspace: IWorkspace;

  sharedDatabaseSession: ISession;

  exclusiveDatabaseWorkspace: IWorkspace;

  exclusiveDatabaseSession: ISession;

  async create<T extends IObject>(
    session: ISession,
    cls: Class,
    mode: DatabaseMode
  ): Promise<T> {
    switch (mode as DatabaseMode) {
      case DatabaseMode.NoPush:
        return session.create<T>(cls);
      case DatabaseMode.Push: {
        const pushObject = session.create<T>(cls);
        await session.push();
        return pushObject;
      }
      case DatabaseMode.PushAndPull: {
        const pushAndPullObject = session.create<T>(cls);
        const result = await session.push();
        if (result.hasErrors) throw new Error();
        await session.pull({ object: pushAndPullObject });
        return pushAndPullObject;
      }
      // case DatabaseMode.SharedDatabase: {
      //   const sharedDatabaseObject = this.sharedDatabaseSession.create<T>(cls);
      //   await this.client.push(this.sharedDatabaseSession);
      //   const sharedResult = await this.client.pull(session, { object: sharedDatabaseObject });
      //   return sharedResult.objects.values().next().value;
      // }
      // case DatabaseMode.ExclusiveDatabase: {
      //   const exclusiveDatabaseObject = this.exclusiveDatabaseSession.create<T>(cls);
      //   await this.client.push(this.exclusiveDatabaseSession);
      //   const exclusiveResult = await this.client.pull(session, { object: exclusiveDatabaseObject });
      //   return exclusiveResult.objects.values().next().value;
      // }
      default:
        throw new Error(mode.toString());
    }
  }
}
