import { Class, RelationType, RoleType } from '@allors/workspace/meta/system';
import { Strategy } from '../Strategy';

export class WorkspaceState {
  workspaceObject: WorkspaceObject;
  changedRoleByRelationType: Map<RelationType, unknown>;

  previousWorkspaceObject: WorkspaceObject;
  previousChangedRoleByRelationType: Map<RelationType, unknown>;

  constructor(private strategy: Strategy) {
    this.workspaceObject = this.workspace.get(this.identity);
    this.previousWorkspaceObject = this.workspaceObject;
  }

  get hasWorkspaceChanges(): boolean {
    return !!this.changedRoleByRelationType;
  }

  get Identity(): number {
    return this.strategy.id;
  }

  get class(): Class {
    return this.strategy.class;
  }

  get Session(): Session {
    return this.strategy.session;
  }

  get Workspace(): Workspace {
    return this.Session.Workspace;
  }

  getRole(roleType: RoleType): unknown {
    // if (roleType.ObjectType.IsUnit)
    // {
    //     if (this.changedRoleByRelationType == null || !this.changedRoleByRelationType.TryGetValue(roleType.RelationType, out var unit))
    //     {
    //         unit = this.workspaceObject?.GetRole(roleType);
    //     }
    //     return unit;
    // }
    // if (roleType.IsOne)
    // {
    //     if (this.changedRoleByRelationType != null &&
    //         this.changedRoleByRelationType.TryGet<Strategy>(roleType.RelationType, out var workspaceRole))
    //     {
    //         return workspaceRole?.Object;
    //     }
    //     var identity = (long?)this.workspaceObject?.GetRole(roleType);
    //     workspaceRole = this.Session.GetStrategy(identity);
    //     return workspaceRole?.Object;
    // }
    // if (this.changedRoleByRelationType != null &&
    //     this.changedRoleByRelationType.TryGet<Strategy[]>(roleType.RelationType, out var workspaceRoles))
    // {
    //     return workspaceRoles != null ? workspaceRoles.Select(v => v.Object).ToArray() : Array.Empty<IObject>();
    // }
    // var identities = (long[])this.workspaceObject?.GetRole(roleType);
    // return identities == null ? Array.Empty<IObject>() : identities.Select(v => this.Session.Get<IObject>(v)).ToArray();
  }

  setUnitRole(roleType: RoleType, role: unknown): void {
    const previousRole = this.getRole(roleType);
    if (previousRole === role) {
      return;
    }

    this.changedRoleByRelationType ??= new Map();
    this.changedRoleByRelationType[roleType.RelationType] = role;

    this.Session.OnChange(this);
  }

  setCompositeRole(roleType: RoleType, value: unknown): void {
    var role = value;
    var previousRole = this.getRole(roleType);
    if (Equals(previousRole, role)) {
      return;
    }

    // OneToOne
    if (previousRole != null) {
      var associationType = roleType.associationType;
      if (associationType.isOne) {
        var previousAssociationObject = this.session.getAssociation<IObject>(previousRole.strategy, associationType);
        previousAssociationObject?.Strategy.Set(roleType, null);
      }
    }

    this.changedRoleByRelationType ??= new Map();
    this.changedRoleByRelationType[roleType.relationType] = role?.strategy;

    this.session.onChange(this);
  }

  setCompositesRole(roleType: RoleType, value: unknown): void {
    const previousRole = this.getRole(roleType);

    const role = [];
    if (value != null) {
      role = value;
    }

    const addedRoles = role.Except(previousRole).ToArray();
    const removedRoles = previousRole.Except(role).ToArray();

    if (addedRoles.Length == 0 && removedRoles.Length == 0) {
      return;
    }

    // OneToMany
    if (previousRole.length > 0) {
      const associationType = roleType.associationType;
      if (associationType.isOne) {
        const addedObjects = this.Session.Get<IObject>(addedRoles);
        for (const addedObject of addedObjects) {
          const previousAssociationObject = this.Session.GetAssociation<IObject>(addedObject.Strategy, associationType);
          previousAssociationObject?.Strategy.Remove(roleType, addedObject);
        }
      }
    }

    this.changedRoleByRelationType ??= new Map();
    this.changedRoleByRelationType[roleType.relationType] = role;

    this.session.onChange(this);
  }

  push(): void {
    if (this.hasWorkspaceChanges) {
      this.Workspace.Push(this.Identity, this.class, this.workspaceObject?.Version ?? 0, this.changedRoleByRelationType);
    }

    this.workspaceObject = this.Workspace.Get(this.Identity);
    this.changedRoleByRelationType = null;
  }

  reset(): void {
    this.workspaceObject = this.Workspace.Get(this.Identity);
    this.changedRoleByRelationType = null;
  }

  checkpoint(changeSet: ChangeSet): void {
    // Same workspace object
    if (this.workspaceObject.Version == this.previousWorkspaceObject.Version) {
      // No previous changed roles
      if (this.previousChangedRoleByRelationType == null) {
        if (this.changedRoleByRelationType != null) {
          // Changed roles
          for (const kvp of this.changedRoleByRelationType) {
            const [relationType, cooked] = kvp;
            const raw = this.workspaceObject.GetRole(relationType.RoleType);

            changeSet.DiffCookedWithRaw(this.strategy, relationType, cooked, raw);
          }
        }
      }
      // Previous changed roles
      else {
        for (const kvp of this.changedRoleByRelationType) {
          const [relationType, role] = kvp;

          const previousRole = this.previousChangedRoleByRelationType.get(relationType);
          changeSet.DiffCookedWithCooked(this.strategy, relationType, role, previousRole);
        }
      }
    }
    // Different workspace objects
    else {
      const hasPreviousCooked = this.previousChangedRoleByRelationType != null;
      const hasCooked = this.changedRoleByRelationType != null;

      for (const roleType in this.class.WorkspaceRoleTypes) {
        const relationType = roleType.relationType;

        if (hasPreviousCooked && this.previousChangedRoleByRelationType.has(relationType)) {
          const previousCooked = this.previousChangedRoleByRelationType.get(relationType);
          if (hasCooked && this.changedRoleByRelationType.has(relationType)) {
            const cooked = this.changedRoleByRelationType.get(relationType);
            changeSet.DiffCookedWithCooked(this.strategy, relationType, cooked, previousCooked);
          } else {
            const raw = this.workspaceObject.GetRole(roleType);
            changeSet.DiffRawWithCooked(this.strategy, relationType, raw, previousCooked);
          }
        } else {
          const previousRaw = this.previousWorkspaceObject?.GetRole(roleType);
          if (hasCooked && this.changedRoleByRelationType.has(relationType)) {
            const cooked = this.changedRoleByRelationType.get(relationType);

            changeSet.DiffCookedWithRaw(this.strategy, relationType, cooked, previousRaw);
          } else {
            const raw = this.workspaceObject.GetRole(roleType);
            changeSet.DiffRawWithRaw(this.strategy, relationType, raw, previousRaw);
          }
        }
      }
    }

    this.previousWorkspaceObject = this.workspaceObject;
    this.previousChangedRoleByRelationType = this.changedRoleByRelationType;
  }

  isAssociationForRole(roleType: RoleType, forRole: Strategy): boolean {
    if (roleType.objectType.isUnit) {
      return false;
    }

    if (roleType.isOne) {
      if (this.changedRoleByRelationType != null && this.changedRoleByRelationType.has(roleType.relationType)) {
        const workspaceRole = this.changedRoleByRelationType.get(roleType.relationType);
        return workspaceRole === forRole;
      }

      const identity = this.workspaceObject?.GetRole(roleType);
      return identity === forRole.id;
    }

    if (this.changedRoleByRelationType != null && this.changedRoleByRelationType.get(roleType.relationType)) {
      const workspaceRoles = this.changedRoleByRelationType.get(roleType.relationType);
      return workspaceRoles.includes(forRole);
    }

    const identities = this.workspaceObject?.GetRole(roleType);
    return identities.includes(forRole.id);
  }

  diff(): RelationType[] {
    if (!this.changedRoleByRelationType) {
      return;
    }

    return Array.from(this.changedRoleByRelationType.keys);
  }
}
