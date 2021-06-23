import { ISession, IWorkspace, IWorkspaceServices } from '@allors/workspace/system';
import { Class, RelationType } from '@allors/workspace/system';
import { Database } from '../Database/Database';
import { Session } from '../Session/Session';
import { WorkspaceRecord } from './WorkspaceRecord';

export class Workspace implements IWorkspace {
  workspaceClassByWorkspaceId: Map<number, Class>;

  workspaceIdsByWorkspaceClass: Map<Class, Set<number>>;

  private readonly recordById: Map<number, WorkspaceRecord>;

  constructor(public database: Database, public services: IWorkspaceServices) {
    this.workspaceClassByWorkspaceId = new Map();
    this.workspaceIdsByWorkspaceClass = new Map();

    this.recordById = new Map();

    this.services.onInit(this);
  }

  createSession(): ISession {
    return new Session(this, this.services.createSessionServices());
  }

  getRecord(identity: number): WorkspaceRecord | undefined {
    return this.recordById.get(identity);
  }

  push(id: number, cls: Class, version: number, changedRoleByRoleType: Map<RelationType, unknown> | undefined): void {
    const originalWorkspaceObject = this.recordById.get(id);
    if (!originalWorkspaceObject) {
      this.recordById.set(id, new WorkspaceRecord(cls, id, ++version, changedRoleByRoleType));
    } else {
      this.recordById.set(id, WorkspaceRecord.fromOriginal(originalWorkspaceObject, changedRoleByRoleType));
    }
  }
}
