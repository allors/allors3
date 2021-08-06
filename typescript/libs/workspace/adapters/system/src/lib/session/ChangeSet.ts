import { IChangeSet, IStrategy, ISession } from '@allors/workspace/domain/system';
import { AssociationType, PropertyType, RelationType, RoleType } from '@allors/workspace/meta/system';
import { MapMap } from '../collections/MapMap';
import { difference, enumerate, IRange } from '../collections/Range';
import { Session } from './Session';
import { Strategy } from './Strategy';
import { frozenEmptySet } from '../collections/frozenEmptySet';

export class ChangeSet implements IChangeSet {
  associationsByRoleType: Map<RoleType, Set<IStrategy>>;
  rolesByAssociationType: Map<AssociationType, Set<IStrategy>>;

  public constructor(public session: ISession, public created: Readonly<Set<IStrategy>> = frozenEmptySet as Readonly<Set<IStrategy>>, public instantiated: Readonly<Set<IStrategy>> = frozenEmptySet as Readonly<Set<IStrategy>>) {
    this.associationsByRoleType = new Map();
    this.rolesByAssociationType = new Map();
  }

  public addSessionStateChanges(sessionStateChangeSet: MapMap<PropertyType, number, unknown>) {
    for (const [propertyType, map] of sessionStateChangeSet.mapMap) {
      const strategies = new Set<IStrategy>();

      for (const [id] of map) {
        const strategy = (this.session as Session).getStrategy(id);
        strategies.add(strategy);
      }

      if (propertyType.isAssociationType) {
        this.rolesByAssociationType.set(propertyType as AssociationType, strategies);
      } else if (propertyType.isRoleType) {
        this.associationsByRoleType.set(propertyType as RoleType, strategies);
      } else {
        throw new Error(`PropertyType ${propertyType.name} is not supported`);
      }
    }
  }

  public diff(association: Strategy, relationType: RelationType, current: unknown, previous: unknown) {
    const roleType = relationType.roleType;

    if (roleType.objectType.isUnit) {
      if (current !== previous) {
        this.addAssociation(relationType, association);
      }
    } else if (roleType.isOne) {
      if (current === previous) {
        return;
      }

      if (previous != null) {
        this.addRole(relationType, (this.session as Session).getStrategy(<number>previous));
      }

      if (current != null) {
        this.addRole(relationType, (this.session as Session).getStrategy(<number>current));
      }

      this.addAssociation(relationType, association);
    } else {
      let hasChange = false;

      const addedRoles = difference(current as IRange, previous as IRange);
      for (const v of enumerate(addedRoles)) {
        this.addRole(relationType, (this.session as Session).getStrategy(v));
        hasChange = true;
      }

      const removedRoles = difference(previous as IRange, current as IRange);
      for (const v of enumerate(removedRoles)) {
        this.addRole(relationType, (this.session as Session).getStrategy(v));
        hasChange = true;
      }

      if (hasChange) {
        this.addAssociation(relationType, association);
      }
    }
  }

  private addAssociation(relationType: RelationType, association: Strategy) {
    const roleType = relationType.roleType;

    let associations: Set<Strategy>;
    if (!this.associationsByRoleType.has(roleType)) {
      associations = new Set();
      this.associationsByRoleType.set(roleType, associations);
    }

    associations.add(association);
  }

  private addRole(relationType: RelationType, role: Strategy) {
    const associationType = relationType.associationType;

    let roles: Set<IStrategy>;
    if (!this.rolesByAssociationType.has(associationType)) {
      roles = new Set();
      this.rolesByAssociationType.set(associationType, roles);
    }

    roles.add(role);
  }
}
