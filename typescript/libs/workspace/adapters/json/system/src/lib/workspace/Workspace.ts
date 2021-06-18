import { ISession, IWorkspace, IWorkspaceServices } from '@allors/workspace/domain/system';
import { Class, RelationType } from '@allors/workspace/meta/system';
import { Database } from '../Database/Database';
import { Session } from '../Session/Session';
import { WorkspaceObject } from './WorkspaceObject';

export class Workspace implements IWorkspace {
  workspaceClassByWorkspaceId: Map<number, Class>;

  workspaceIdsByWorkspaceClass: Map<Class, Set<number>>;

  private readonly objectById: Map<number, WorkspaceObject>;

  constructor(public database: Database, public services: IWorkspaceServices) {
    this.workspaceClassByWorkspaceId = new Map();
    this.workspaceIdsByWorkspaceClass = new Map();

    this.objectById = new Map();

    this.services.onInit(this);
  }

  createSession(): ISession {
    return new Session(this, this.services.createSessionServices());
  }

  get(identity: number): WorkspaceObject | undefined {
    return this.objectById.get(identity);
  }

  registerWorkspaceObject(cls: Class, workspaceId: number): void {
    this.workspaceClassByWorkspaceId.set(workspaceId, cls);

    let ids = this.workspaceIdsByWorkspaceClass.get(cls);
    if (ids === undefined) {
      ids = new Set();
      this.workspaceIdsByWorkspaceClass.set(cls, ids);
    }

    ids.add(workspaceId);
  }

  push(identity: number, cls: Class, version: number, changedRoleByRoleType: Map<RelationType, unknown> | undefined): void {
    const originalWorkspaceObject = this.objectById.get(identity);
    if (!originalWorkspaceObject) {
      this.objectById.set(identity, new WorkspaceObject(this.database, identity, cls, ++version, changedRoleByRoleType));
    } else {
      this.objectById.set(identity, WorkspaceObject.fromOriginal(originalWorkspaceObject, changedRoleByRoleType));
    }
  }
}
