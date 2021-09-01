import { IChangeSet, IStrategy, ISession } from '@allors/workspace/domain/system';
import { AssociationType, PropertyType, RelationType, RoleType } from '@allors/workspace/meta/system';
import { MapMap } from '../collections/MapMap';
import { Session } from './Session';
import { Strategy } from './Strategy';
import { frozenEmptySet } from '../collections/frozenEmptySet';
import { IRange, Ranges } from '../collections/ranges/Ranges';

export class ChangeSet implements IChangeSet {
  associationsByRoleType: Map<RoleType, Set<IStrategy>>;
  rolesByAssociationType: Map<AssociationType, Set<IStrategy>>;

  private ranges: Ranges<Strategy>;

  public constructor(public session: Session, public created: Set<IStrategy>, public instantiated: Set<IStrategy>) {
    this.associationsByRoleType = new Map();
    this.rolesByAssociationType = new Map();

    this.created ??= frozenEmptySet as Set<IStrategy>;
    this.instantiated ??= frozenEmptySet as Set<IStrategy>;

    this.ranges = this.session.ranges;
  }

  public addSessionStateChanges(sessionStateChangeSet: MapMap<PropertyType, Strategy, unknown>) {
    for (const [propertyType, map] of sessionStateChangeSet.mapMap) {
      const strategies = new Set<IStrategy>();

      for (const [strategy] of map) {
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

  public diffCompositeStrategyRecord(association: Strategy, relationType: RelationType, current: Strategy, previous: number) {
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

  public diffCompositeRecordRecord(association: Strategy, relationType: RelationType, current: number, previous: number) {
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

  public diffCompositesStrategyRecord(association: Strategy, relationType: RelationType, current: IRange<Strategy>, previousRange: IRange<number>) {
    const previous: IRange<Strategy> = previousRange?.map((v) => (this.session as Session).getStrategy(v));
    this.diffCompositesStrategyStrategy(association, relationType, current, previous);
  }

  public diffCompositesRecordRecord(association: Strategy, relationType: RelationType, currentRange: IRange<number>, previousRange: IRange<number>) {
    const current: IRange<Strategy> = currentRange?.map((v) => (this.session as Session).getStrategy(v));
    const previous: IRange<Strategy> = previousRange?.map((v) => (this.session as Session).getStrategy(v));
    this.diffCompositesStrategyStrategy(association, relationType, current, previous);
  }

  public diffCompositesStrategyStrategy(association: Strategy, relationType: RelationType, current: IRange<Strategy>, previous: IRange<Strategy>) {
    let hasChange = false;

    for (const v of this.ranges.enumerate(previous)) {
      if (!this.ranges.has(current, v)) {
        this.addRole(relationType, v);
        hasChange = true;
      }
    }

    for (const v of this.ranges.enumerate(current)) {
      if (!this.ranges.has(previous, v)) {
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
