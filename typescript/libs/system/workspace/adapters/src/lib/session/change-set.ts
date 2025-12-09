import { IChangeSet, IObject } from '@allors/system/workspace/domain';
import {
  AssociationType,
  PropertyType,
  RelationType,
  RoleType,
} from '@allors/system/workspace/meta';

import { MapMap } from '../collections/map-map';
import { frozenEmptySet } from '../collections/frozen-empty-set';
import { IRange, Ranges } from '../collections/ranges/ranges';

import { Session } from './session';

export class ChangeSet implements IChangeSet {
  associationsByRoleType: Map<RoleType, Set<IObject>>;
  rolesByAssociationType: Map<AssociationType, Set<IObject>>;

  private ranges: Ranges<IObject>;

  public constructor(public session: Session, public created: Set<IObject>) {
    this.associationsByRoleType = new Map();
    this.rolesByAssociationType = new Map();
    this.created ??= frozenEmptySet as Set<IObject>;

    this.ranges = this.session.ranges;
  }

  public addSessionStateChanges(
    sessionStateChangeSet: MapMap<PropertyType, IObject, unknown>
  ) {
    for (const [propertyType, map] of sessionStateChangeSet.mapMap) {
      const strategies = new Set<IObject>();

      for (const [strategy] of map) {
        strategies.add(strategy);
      }

      if (propertyType.isAssociationType) {
        this.rolesByAssociationType.set(
          propertyType as AssociationType,
          strategies
        );
      } else if (propertyType.isRoleType) {
        this.associationsByRoleType.set(propertyType as RoleType, strategies);
      } else {
        throw new Error(`PropertyType ${propertyType.name} is not supported`);
      }
    }
  }

  public diffUnit(
    association: IObject,
    relationType: RelationType,
    current: unknown,
    previous: unknown
  ) {
    if (current !== previous) {
      this.addAssociation(relationType, association);
    }
  }

  public diffCompositeStrategyRecord(
    association: IObject,
    relationType: RelationType,
    current: IObject,
    previous: number
  ) {
    if (current?.id === previous) {
      return;
    }

    if (previous != null) {
      const previousStrategy = (this.session as Session).getObject(previous);
      if (previousStrategy) {
        this.addRole(relationType, previousStrategy);
      }
    }

    if (current != null) {
      this.addRole(relationType, current);
    }

    this.addAssociation(relationType, association);
  }

  public diffCompositeRecordRecord(
    association: IObject,
    relationType: RelationType,
    current: number,
    previous: number
  ) {
    if (current === previous) {
      return;
    }

    if (previous != null) {
      this.addRole(relationType, (this.session as Session).getObject(previous));
    }

    if (current != null) {
      this.addRole(relationType, (this.session as Session).getObject(current));
    }

    this.addAssociation(relationType, association);
  }

  public diffCompositeStrategyStrategy(
    association: IObject,
    relationType: RelationType,
    current: IObject,
    previous: IObject
  ) {
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

  public diffCompositesStrategyRecord(
    association: IObject,
    relationType: RelationType,
    current: IRange<IObject>,
    previousRange: IRange<number>
  ) {
    const previous: IRange<IObject> = previousRange?.map((v) =>
      (this.session as Session).getObject(v)
    );
    this.diffCompositesStrategyStrategy(
      association,
      relationType,
      current,
      previous
    );
  }

  public diffCompositesRecordRecord(
    association: IObject,
    relationType: RelationType,
    currentRange: IRange<number>,
    previousRange: IRange<number>
  ) {
    const current: IRange<IObject> = currentRange?.map((v) =>
      (this.session as Session).getObject(v)
    );
    const previous: IRange<IObject> = previousRange?.map((v) =>
      (this.session as Session).getObject(v)
    );
    this.diffCompositesStrategyStrategy(
      association,
      relationType,
      current,
      previous
    );
  }

  public diffCompositesStrategyStrategy(
    association: IObject,
    relationType: RelationType,
    current: IRange<IObject>,
    previous: IRange<IObject>
  ) {
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

  private addAssociation(relationType: RelationType, association: IObject) {
    if (association == null) {
      // TODO: Investigate
      return;
    }

    const roleType = relationType.roleType;

    let associations = this.associationsByRoleType.get(roleType);
    if (!associations) {
      associations = new Set();
      this.associationsByRoleType.set(roleType, associations);
    }

    associations.add(association);
  }

  private addRole(relationType: RelationType, role: IObject) {
    if (role == null) {
      // TODO: Investigate
      return;
    }

    const associationType = relationType.associationType;

    let roles = this.rolesByAssociationType.get(associationType);
    if (!roles) {
      roles = new Set();
      this.rolesByAssociationType.set(associationType, roles);
    }

    roles.add(role);
  }
}
