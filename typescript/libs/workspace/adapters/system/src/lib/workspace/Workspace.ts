import { ISession, IWorkspace, IWorkspaceServices } from '@allors/workspace/domain/system';
import { Class, RelationType } from '@allors/workspace/meta/system';
import { Database } from '../Database/Database';
import { WorkspaceRecord } from './WorkspaceRecord';

export abstract class Workspace implements IWorkspace {
  workspaceClassByWorkspaceId: Map<number, Class>;

  workspaceIdsByWorkspaceClass: Map<Class, Set<number>>;

  private readonly recordById: Map<number, WorkspaceRecord>;

  constructor(public database: Database, public services: IWorkspaceServices) {
    this.workspaceClassByWorkspaceId = new Map();
    this.workspaceIdsByWorkspaceClass = new Map();

    this.recordById = new Map();
  }

  abstract createSession(): ISession;

  getRecord(identity: number): WorkspaceRecord | undefined {
    return this.recordById.get(identity);
  }

  push(id: number, cls: Class, version: number, changedRoleByRoleType: Map<RelationType, unknown> | undefined): void {
    this.workspaceClassByWorkspaceId.set(id, cls);
    let ids = this.workspaceIdsByWorkspaceClass.get(cls);
    if (ids == null) {
      ids = new Set();
      this.workspaceIdsByWorkspaceClass.set(cls, ids);
    }

    ids.add(id);

    const originalWorkspaceObject = this.recordById.get(id);
    if (originalWorkspaceObject == null) {
      this.recordById.set(id, new WorkspaceRecord(cls, id, ++version, changedRoleByRoleType));
    } else {
      this.recordById.set(id, WorkspaceRecord.fromOriginal(originalWorkspaceObject, changedRoleByRoleType));
    }
  }
}
