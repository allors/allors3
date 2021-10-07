import { IConfiguration, IRule, ISession, IWorkspace } from '@allors/workspace/domain/system';
import { Class, RelationType } from '@allors/workspace/meta/system';

import { Ranges } from '../collections/ranges/ranges';
import { DatabaseConnection } from '../database/database-connection';
import { Strategy } from '../session/strategy';
import { WorkspaceRecord } from './workspace-record';

export abstract class Workspace implements IWorkspace {
  configuration: IConfiguration;

  workspaceClassByWorkspaceId: Map<number, Class>;

  workspaceIdsByWorkspaceClass: Map<Class, Set<number>>;

  readonly ranges: Ranges<number>;

  private readonly recordById: Map<number, WorkspaceRecord>;

  constructor(public database: DatabaseConnection) {
    this.ranges = database.ranges;

    this.configuration = database.configuration;
    this.workspaceClassByWorkspaceId = new Map();
    this.workspaceIdsByWorkspaceClass = new Map();

    this.recordById = new Map();
  }

  get rules(): Readonly<IRule[]> {
    return this.database.configuration.engine.rules;
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

    let roleByRelationType: Map<RelationType, any>;
    if (changedRoleByRoleType?.size > 0) {
      roleByRelationType = new Map();
      for (const [key, value] of changedRoleByRoleType) {
        if (value instanceof Strategy) {
          roleByRelationType.set(key, value.id);
        } else if (value instanceof Set) {
          roleByRelationType.set(key, this.ranges.importFrom([...(value as Set<Strategy>)].map((v) => v.id)));
        } else {
          roleByRelationType.set(key, value);
        }
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
