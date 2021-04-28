import { PushRequestRole } from '@allors/protocol/json/system';
import { IObject, ISession } from '@allors/workspace/domain/system';
import { Class, MethodType, RelationType, RoleType } from '@allors/workspace/meta/system';
import { ChangeSet } from '../ChangeSet';
import { Strategy } from '../Strategy';

export class DatabaseState {
  private changedRoleByRelationType: Map<RelationType, any>;

  private previousDatabaseObject: DatabaseObject;
  private previousChangedRoleByRelationType: Map<RelationType, object>;

  constructor(private readonly strategy: Strategy, public databaseObject?: DatabaseObject) {
    this.strategy = strategy;
    this.databaseObject = databaseObject ?? this.database.Get(this.identity);
    this.previousDatabaseObject = this.databaseObject;
  }

  get hasDatabaseChanges(): boolean {
    return !!this.databaseObject || !!this.changedRoleByRelationType;
  }

  get existDatabaseObjects(): boolean {
    return !!this.databaseObject;
  }

  get version(): number {
    return this.databaseObject?.Version ?? 0;
  }

  get identity(): number {
    return this.strategy.id;
  }

  get Class(): Class {
    return this.strategy.class;
  }

  get session(): ISession {
    return this.strategy.session;
  }

  get Database(): Database {
    return this.session.database;
  }

  canRead(roleType: RoleType): boolean {
    if (!this.existDatabaseObjects) {
      return true;
    }

    let permission = this.session.workspace.database.getPermission(this.class, roleType, Operations.Read);
    return this.databaseObject.IsPermitted(permission);
  }

  canWrite(roleType: RoleType): boolean {
    if (!this.existDatabaseObjects) {
      return true;
    }

    let permission = this.session.workspace.database.getPermission(this.Class, roleType, Operations.Write);
    return this.databaseObject.IsPermitted(permission);
  }

  canExecute(methodType: MethodType): boolean {
    if (!this.existDatabaseObjects) {
      return true;
    }

    let permission = this.session.workspace.database.getPermission(this.Class, methodType, Operations.Execute);
    return this.databaseObject.IsPermitted(permission);
  }

  getRole(roleType: RoleType): unknown {
    if (roleType.objectType.isUnit) {
      return this.changedRoleByRelationType?.get(roleType.relationType);
    }

    if (roleType.isOne) {
      if (this.changedRoleByRelationType?.has(roleType.relationType)) {
        return (this.changedRoleByRelationType?.get(roleType.relationType) as Strategy)?.object;
      }

      let identity = this.databaseObject?.getRole(roleType);
      workspaceRole = this.session.getStrategy(identity);

      return workspaceRole?.Object;
    }

    if (this.changedRoleByRelationType?.has(roleType.relationType)) {
      const workspaceRoles = this.changedRoleByRelationType?.get(roleType.relationType) as Strategy[];
      return workspaceRoles != null ? workspaceRoles.map((v) => v.object) : [];
    }

    let identities = this.databaseObject?.getRole(roleType);
    return !!identities ? identities.map((v) => this.session.getMany<IObject>(v)) : [];
  }

  setUnitRole(roleType: RoleType, role: unknown): void {
    let previousRole = this.getRole(roleType);
    if (Equals(previousRole, role)) {
      return;
    }

    this.changedRoleByRelationType ??= new Map();
    this.changedRoleByRelationType.set(roleType.relationType, role);

    this.session.onChange(this);
  }

  setCompositeRole(roleType: RoleType, value: IObject) {
    let role = value;
    let previousRole = this.getRole(roleType);
    if (previousRole === role) {
      return;
    }

    // OneToOne
    if (previousRole != null) {
      let associationType = roleType.associationType;
      if (associationType.isOne) {
        let previousAssociationObject = this.session.getAssociation<IObject>(previousRole.strategy, associationType);
        previousAssociationObject?.Strategy.Set(roleType, null);
      }
    }

    this.changedRoleByRelationType ??= new Map();
    this.changedRoleByRelationType.set(roleType.relationType, role?.strategy);

    this.session.onChange(this);
  }

  setCompositesRole(roleType: RoleType, value: IObject[]): void {
    let previousRole = this.getRole(roleType) as IObject[];

    let role: IObject[] = [];
    if (value != null) {
      role = value;
    }

    let addedRoles = role.filter((v) => !previousRole.includes(v));
    let removedRoles = previousRole.filter((v) => !role.includes(v));

    if (addedRoles.length === 0 && removedRoles.length === 0) {
      return;
    }

    // OneToMany
    if (previousRole.length > 0) {
      let associationType = roleType.associationType;
      if (associationType.isOne) {
        let addedObjects = this.session.getMany<IObject>(addedRoles);
        for (let addedObject of addedObjects) {
          let previousAssociationObject = this.session.getAssociation<IObject>(addedObject.strategy, associationType);
          previousAssociationObject?.Strategy.Remove(roleType, addedObject);
        }
      }
    }

    this.changedRoleByRelationType ??= new Map();
    this.changedRoleByRelationType.set(
      roleType.relationType,
      role.map((v) => v.strategy)
    );

    this.session.onChange(this);
  }

  reset(): void {
    this.databaseObject = this.database.get(this.identity);
    this.changedRoleByRelationType = null;
  }

  checkpoint(changeSet: ChangeSet): void {
    // Same workspace object
    if (this.databaseObject.Version == this.previousDatabaseObject.Version) {
      // No previous changed roles
      if (this.previousChangedRoleByRelationType == null) {
        if (this.changedRoleByRelationType != null) {
          // Changed roles
          for (const kvp of this.changedRoleByRelationType) {
            const [relationType, cooked] = kvp;
            var raw = this.databaseObject.GetRole(relationType.RoleType);

            changeSet.diffCookedWithRaw(this.strategy, relationType, cooked, raw);
          }
        }
      }
      // Previous changed roles
      else {
        for (const kvp in this.changedRoleByRelationType) {
          const [relationType, role] = kvp;

          const previousRole = this.previousChangedRoleByRelationType.get(relationType);
          changeSet.diffCookedWithCooked(this.strategy, relationType, role, previousRole);
        }
      }
    }
    // Different workspace objects
    else {
      let hasPreviousCooked = this.previousChangedRoleByRelationType != null;
      let hasCooked = this.changedRoleByRelationType != null;

      for (const roleType in this.class.workspaceRoleTypes) {
        let relationType = roleType.relationType;

        if (hasPreviousCooked && this.previousChangedRoleByRelationType.has(relationType)) {
          const previousCooked = this.previousChangedRoleByRelationType.get(relationType);
          if (hasCooked && this.changedRoleByRelationType.has(relationType)) {
            const cooked = this.changedRoleByRelationType.get(relationType);
            changeSet.diffCookedWithCooked(this.strategy, relationType, cooked, previousCooked);
          } else {
            var raw = this.databaseObject.GetRole(roleType);
            changeSet.diffRawWithCooked(this.strategy, relationType, raw, previousCooked);
          }
        } else {
          let previousRaw = this.previousDatabaseObject?.GetRole(roleType);
          if (hasCooked && this.changedRoleByRelationType.has(relationType)) {
            const cooked = this.changedRoleByRelationType.get(relationType);
            changeSet.diffCookedWithRaw(this.strategy, relationType, cooked, previousRaw);
          } else {
            var raw = this.databaseObject.GetRole(roleType);
            changeSet.diffRawWithRaw(this.strategy, relationType, raw, previousRaw);
          }
        }
      }
    }

    this.previousDatabaseObject = this.databaseObject;
    this.previousChangedRoleByRelationType = this.changedRoleByRelationType;
  }

  pushResponse(newDatabaseObject: DatabaseObject): void {
    this.databaseObject = newDatabaseObject;
  }

  pushNew(): PushRequestNewObject {
    return {
      WorkspaceId = this.Identity,
      ObjectType = this.Class.Tag,
      Roles = this.pushRoles(),
    };
  }

  pushExisting(): PushRequestObject {
    return {
      DatabaseId = this.identity,
      Version = this.bersion,
      Roles = this.PushRoles(),
    };
  }

  pushRoles(): PushRequestRole[] {
    if (this.changedRoleByRelationType?.length > 0) {
      const roles: PushRequestRole[] = [];

      for (const kvp of this.changedRoleByRelationType) {
        const [relationType, roleValue] = kvp;

        let pushRequestRole: PushRequestRole = { t: relationType.tag };

        if (relationType.roleType.objectType.isUnit) {
          pushRequestRole.u = UnitConvert.toString(roleValue);
        } else {
          if (relationType.roleType.isOne) {
            pushRequestRole.c = roleValue?.Id;
          } else {
            let roleIds = roleValue.map((v) => v.id);
            if (!this.existDatabaseObjects) {
              pushRequestRole.a = roleIds;
            } else {
              let databaseRole = this.databaseObject.getRole(relationType.roleType);
              if (databaseRole == null) {
                pushRequestRole.a = roleIds;
              } else {
                pushRequestRole.a = roleIds.Except(databaseRole).ToArray();
                pushRequestRole.r = databaseRole.Except(roleIds).ToArray();
              }
            }
          }
        }

        roles.push(pushRequestRole);
      }

      return roles;
    }

    return null;
  }

  isAssociationForRole(roleType: RoleType, forRole: Strategy): boolean {
    if (roleType.objectType.isUnit) {
      return false;
    }

    if (roleType.isOne) {
      if (this.changedRoleByRelationType?.has(roleType.relationType)) {
        const workspaceRole = this.changedRoleByRelationType.get(roleType.relationType);
        return workspaceRole?.Equals(forRole) == true;
      }

      const identity = this.databaseObject?.getRole(roleType);
      return identity === forRole.id;
    }

    if (this.changedRoleByRelationType.has(roleType.relationType)) {
      const workspaceRoles = this.changedRoleByRelationType.get(roleType.relationType);
      return workspaceRoles?.Contains(forRole) == true;
    }

    const identities = this.databaseObject?.GetRole(roleType);
    return identities?.includes(forRole.id);
  }

  diff(): RelationType[] {
    if (!this.changedRoleByRelationType) {
      return [];
    }

    return this.changedRoleByRelationType.keys;
  }
}
