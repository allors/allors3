import {
  Configuration,
  IObject,
  IRule,
  ISession,
  IWorkspace,
} from '@allors/system/workspace/domain';
import {
  Class,
  Composite,
  RelationType,
  RoleType,
} from '@allors/system/workspace/meta';

import { Ranges } from '../collections/ranges/ranges';
import { DatabaseConnection } from '../database/database-connection';
import { Strategy } from '../session/strategy';
import { WorkspaceRecord } from './workspace-record';

export abstract class Workspace implements IWorkspace {
  configuration: Configuration;

  ruleByRoleType: Map<RoleType, IRule<IObject>>;
  rulesByClassByRoleType: Map<RoleType, Map<Class, IRule<IObject>>>;

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

    this.ruleByRoleType = new Map();
    this.rulesByClassByRoleType = new Map();

    for (const rule of this.configuration.rules) {
      Object.freeze(rule);

      const roleType = rule.roleType;

      if (roleType.associationType.objectType.isClass) {
        this.ruleByRoleType.set(roleType, rule);
      } else {
        let ruleByClass = this.rulesByClassByRoleType.get(roleType);
        if (ruleByClass == null) {
          ruleByClass = new Map();
          this.rulesByClassByRoleType.set(roleType, ruleByClass);
        }

        const objectType = rule.objectType;
        for (const cls of objectType.classes) {
          ruleByClass.set(cls, rule);
        }
      }
    }
  }

  abstract createSession(): ISession;

  rule(roleType: RoleType, strategy: Strategy): IRule<IObject> {
    if (roleType.associationType.objectType.isClass) {
      return this.ruleByRoleType.get(roleType);
    } else {
      return this.rulesByClassByRoleType.get(roleType)?.get(strategy.cls);
    }
  }

  getRecord(id: number): WorkspaceRecord | undefined {
    return this.recordById.get(id);
  }

  push(
    id: number,
    cls: Class,
    version: number,
    changedRoleByRoleType: Map<RelationType, unknown> | undefined
  ): void {
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
          roleByRelationType.set(
            key,
            this.ranges.importFrom(
              [...(value as Set<Strategy>)].map((v) => v.id)
            )
          );
        } else {
          roleByRelationType.set(key, value);
        }
      }
    }

    const originalWorkspaceRecord = this.recordById.get(id);
    if (originalWorkspaceRecord == null) {
      this.recordById.set(
        id,
        new WorkspaceRecord(cls, id, ++version, roleByRelationType)
      );
    } else {
      this.recordById.set(
        id,
        WorkspaceRecord.fromOriginal(
          originalWorkspaceRecord,
          roleByRelationType
        )
      );
    }
  }
}
