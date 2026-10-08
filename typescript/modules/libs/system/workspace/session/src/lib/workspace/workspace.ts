import {
  createIdGenerator,
  IDatabaseConnection,
  IdGenerator,
  Ranges,
} from '@allors/system/workspace/connection';
import {
  Configuration,
  IObject,
  IObjectFactory,
  IRule,
  ISession,
  IWorkspace,
} from '@allors/system/workspace/domain';
import { Class, RoleType } from '@allors/system/workspace/meta';

import { Session } from '../session/session';
import { Strategy } from '../session/strategy';

/**
 * A workspace over a connection: it creates sessions, which build objects on the records of
 * the connection, and gives new objects their ids. The name and the meta population of its
 * configuration are the connection's; the object factory and the rules are its own.
 */
export class Workspace implements IWorkspace {
  readonly configuration: Configuration;

  readonly ranges: Ranges<number>;

  readonly idGenerator: IdGenerator;

  private readonly ruleByRoleType: Map<RoleType, IRule<IObject>>;

  private readonly rulesByClassByRoleType: Map<RoleType, Map<Class, IRule<IObject>>>;

  constructor(
    public readonly connection: IDatabaseConnection,
    objectFactory: IObjectFactory,
    rules?: IRule<IObject>[],
    idGenerator?: IdGenerator
  ) {
    if (connection == null) {
      throw new Error('A workspace needs a connection.');
    }

    if (objectFactory == null) {
      throw new Error('A workspace needs an object factory.');
    }

    this.ranges = connection.ranges;
    this.idGenerator = idGenerator ?? createIdGenerator();

    this.configuration = {
      name: connection.workspaceName,
      metaPopulation: connection.metaPopulation,
      objectFactory,
      rules: rules ?? [],
    };

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

  createSession(): ISession {
    return new Session(this);
  }

  rule(roleType: RoleType, strategy: Strategy): IRule<IObject> {
    if (roleType.associationType.objectType.isClass) {
      return this.ruleByRoleType.get(roleType);
    }

    return this.rulesByClassByRoleType.get(roleType)?.get(strategy.cls);
  }
}
