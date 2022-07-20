import {
  Configuration,
  IObject,
  IRule,
  ISession,
  IWorkspace,
} from '@allors/system/workspace/domain';
import { Class, RoleType } from '@allors/system/workspace/meta';

import { Ranges } from '../collections/ranges/ranges';
import { DatabaseConnection } from '../database/database-connection';
import { Strategy } from '../session/strategy';

export abstract class Workspace implements IWorkspace {
  configuration: Configuration;

  ruleByRoleType: Map<RoleType, IRule<IObject>>;
  rulesByClassByRoleType: Map<RoleType, Map<Class, IRule<IObject>>>;

  readonly ranges: Ranges<number>;

  constructor(public database: DatabaseConnection) {
    this.ranges = database.ranges;

    this.configuration = database.configuration;

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
    }

    return this.rulesByClassByRoleType.get(roleType)?.get(strategy.cls);
  }
}
