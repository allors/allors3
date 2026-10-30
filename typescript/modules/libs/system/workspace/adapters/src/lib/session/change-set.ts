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
import { Workspace } from '../workspace/workspace';

export class ChangeSet implements IChangeSet {
  associationsByRoleType: Map<RoleType, Set<IObject>>;
  rolesByAssociationType: Map<AssociationType, Set<IObject>>;
  roleIdsByAssociationType: Map<AssociationType, Set<number>>;

  private ranges: Ranges<number>;

  public constructor(public session: Session, public created: Set<IObject>) {
    this.associationsByRoleType = new Map();
    this.rolesByAssociationType = new Map();
    this.roleIdsByAssociationType = new Map();
    this.created ??= frozenEmptySet as Set<IObject>;

    this.ranges = (this.session.workspace as Workspace).ranges;
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
        this.roleIdsByAssociationType.set(
          propertyType as AssociationType,
          new Set([...strategies].map((v) => v.id))
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

  public diffComposite(
    association: IObject,
    relationType: RelationType,
    current: number,
    previous: number
  ) {
    if (current === previous) {
      return;
    }

    if (previous !== 0) {
      this.addRoleId(relationType, previous);
    }

    if (current !== 0) {
      this.addRoleId(relationType, current);
    }

    this.addAssociation(relationType, association);
  }

  public diffComposites(
    association: IObject,
    relationType: RelationType,
    current: IRange<number>,
    previous: IRange<number>
  ) {
    let hasChange = false;

    for (const v of this.ranges.enumerate(previous)) {
      if (!this.ranges.has(current, v)) {
        this.addRoleId(relationType, v);
        hasChange = true;
      }
    }

    for (const v of this.ranges.enumerate(current)) {
      if (!this.ranges.has(previous, v)) {
        this.addRoleId(relationType, v);
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

  // The id is always recorded; the object only when this session holds it.
  private addRoleId(relationType: RelationType, id: number) {
    const associationType = relationType.associationType;

    let roleIds = this.roleIdsByAssociationType.get(associationType);
    if (!roleIds) {
      roleIds = new Set();
      this.roleIdsByAssociationType.set(associationType, roleIds);
    }

    roleIds.add(id);

    this.addRole(relationType, this.session.getObject(id));
  }

  private addRole(relationType: RelationType, role: IObject) {
    if (role == null) {
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
