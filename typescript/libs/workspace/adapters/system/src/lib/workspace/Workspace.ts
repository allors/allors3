import { IConfiguration, ISession, IWorkspace, IWorkspaceServices } from '@allors/workspace/domain/system';
import { Class, RelationType } from '@allors/workspace/meta/system';
import { importFrom } from '../collections/Range';
import { DatabaseConnection } from '../Database/DatabaseConnection';
import { Strategy } from '../session/Strategy';
import { WorkspaceRecord } from './WorkspaceRecord';

export abstract class Workspace implements IWorkspace {
  configuration: IConfiguration;

  workspaceClassByWorkspaceId: Map<number, Class>;

  workspaceIdsByWorkspaceClass: Map<Class, Set<number>>;

  private readonly recordById: Map<number, WorkspaceRecord>;

  constructor(public database: DatabaseConnection, public services: IWorkspaceServices) {
    this.configuration = database.configuration;
    this.workspaceClassByWorkspaceId = new Map();
    this.workspaceIdsByWorkspaceClass = new Map();

    this.recordById = new Map();
  }

  abstract createSession(): ISession;

  getRecord(id: number): WorkspaceRecord | undefined {
    return this.recordById.get(id);
  }

  push(id: number, cls: Class, version: number, changedRoleByRoleType: Map<RelationType, unknown> | undefined): void {
    this.workspaceClassByWorkspaceId.set(id, cls);
    let ids = this.workspaceIdsByWorkspaceClass.get(cls);
    if (ids == null) {
      ids = new Set();
      this.workspaceIdsByWorkspaceClass.set(cls, ids);
    }

    ids.add(id);

    const roleByRelationType = new Map();
    for (const [key, value] of changedRoleByRoleType) {
      if (value instanceof Strategy) {
        roleByRelationType.set(key, value.id);
      } else if (value instanceof Set) {
        roleByRelationType.set(key, importFrom([...(value as Set<Strategy>)].map((v) => v.id)));
      } else {
        roleByRelationType.set(key, value);
      }
    }

    const originalWorkspaceRecord = this.recordById.get(id);
    if (originalWorkspaceRecord == null) {
      this.recordById.set(id, new WorkspaceRecord(cls, id, ++version, roleByRelationType));
    } else {
      this.recordById.set(id, WorkspaceRecord.fromOriginal(originalWorkspaceRecord, roleByRelationType));
    }
  }
}
