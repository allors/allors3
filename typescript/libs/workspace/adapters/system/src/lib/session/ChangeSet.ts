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

  public constructor(public session: ISession, public created: Set<IStrategy>, public instantiated: Set<IStrategy>) {
    this.associationsByRoleType = new Map();
    this.rolesByAssociationType = new Map();

    this.created ??= frozenEmptySet as Set<IStrategy>;
    this.instantiated ??= frozenEmptySet as Set<IStrategy>;
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

  public diffUnit(association: Strategy, relationType: RelationType, current: unknown, previous: unknown) {
    if (current !== previous) {
      this.addAssociation(relationType, association);
    }
  }

  public diffCompositeStrategyNumber(association: Strategy, relationType: RelationType, current: Strategy, previous: number) {
    if (current?.id === previous) {
      return;
    }

    if (previous != null) {
      this.addRole(relationType, (this.session as Session).getStrategy(previous));
    }

    if (current != null) {
      this.addRole(relationType, current);
    }

    this.addAssociation(relationType, association);
  }

  public diffCompositeNumberNumber(association: Strategy, relationType: RelationType, current: number, previous: number) {
    if (current === previous) {
      return;
    }

    if (previous != null) {
      this.addRole(relationType, (this.session as Session).getStrategy(previous));
    }

    if (current != null) {
      this.addRole(relationType, (this.session as Session).getStrategy(current));
    }

    this.addAssociation(relationType, association);
  }

  public diffCompositeStrategyStrategy(association: Strategy, relationType: RelationType, current: Strategy, previous: Strategy) {
    if (current === previous) {
      return;
    }

    if (previous != null) {
      this.addRole(relationType, previous);
    }

    if (current != null) {
      this.addRole(relationType, current);
    }

    this.addAssociation(relationType, association);
  }

  public diffCompositesSetRange(association: Strategy, relationType: RelationType, current: Set<Strategy>, previousRange: IRange) {
    const previous: Set<Strategy> = new Set(previousRange?.map((v) => (this.session as Session).getStrategy(v)));
    this.diffCompositesSetSet(association, relationType, current, previous);
  }

  public diffCompositesRangeRange(association: Strategy, relationType: RelationType, currentRange: IRange, previousRange: IRange) {
    const current: Set<Strategy> = new Set(currentRange?.map((v) => (this.session as Session).getStrategy(v)));
    const previous: Set<Strategy> = new Set(previousRange?.map((v) => (this.session as Session).getStrategy(v)));
    this.diffCompositesSetSet(association, relationType, current, previous);
  }

  public diffCompositesSetSet(association: Strategy, relationType: RelationType, current: Set<Strategy>, previous: Set<Strategy>) {
    let hasChange = false;

    for (const v of previous) {
      if (!current.has(v)) {
        this.addRole(relationType, v);
        hasChange = true;
      }
    }

    for (const v of current) {
      if (!previous.has(v)) {
        this.addRole(relationType, v);
        hasChange = true;
      }
    }

    if (hasChange) {
      this.addAssociation(relationType, association);
    }
  }

  private addAssociation(relationType: RelationType, association: Strategy) {
    const roleType = relationType.roleType;

    let associations = this.associationsByRoleType.get(roleType);
    if (!associations) {
      associations = new Set();
      this.associationsByRoleType.set(roleType, associations);
    }

    associations.add(association);
  }

  private addRole(relationType: RelationType, role: Strategy) {
    const associationType = relationType.associationType;

    let roles = this.rolesByAssociationType.get(associationType);
    if (!roles) {
      roles = new Set();
      this.rolesByAssociationType.set(associationType, roles);
    }

    roles.add(role);
  }
}
