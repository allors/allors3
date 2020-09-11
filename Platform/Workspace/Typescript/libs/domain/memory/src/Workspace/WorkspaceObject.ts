import { ObjectType, AssociationType, RoleType, MethodType, OperandType, unitIds } from '@allors/meta/system';
import { Operations, PushRequestObject, PushRequestNewObject, PushRequestRole } from '@allors/protocol/system';
import { DatabaseObject, WorkspaceObject, UnitTypes, serialize, Method } from '@allors/domain/system';

import { MemoryWorkspace } from './Workspace';

export function deserialize(value: string, objectType: ObjectType): UnitTypes {
  switch (objectType.id) {
    case unitIds.Boolean:
      return value === 'true' ? true : false;
    case unitIds.Float:
      return parseFloat(value);
    case unitIds.Integer:
      return parseInt(value, 10);
  }

  return value;
}

export abstract class MemoryWorkspaceObject implements WorkspaceObject {
  objectType!: ObjectType;
  workspace: MemoryWorkspace

  newId?: string;
  private changedRoleByRoleType?: Map<RoleType, any>;
  private roleByRoleType?: Map<RoleType, any>;
  public databaseObject?: DatabaseObject;

  get isNew(): boolean {
    return this.newId ? true : false;
  }

  get hasChanges(): boolean {
    if (this.newId) {
      return true;
    }

    return !!this.changedRoleByRoleType;
  }

  get id(): string {
    return this.databaseObject ? this.databaseObject.id : this.newId!;
  }

  get version(): string | undefined {
    return this.databaseObject?.version;
  }

  public canRead(roleType: RoleType): boolean | undefined {
    return this.isPermited(roleType, Operations.Read);
  }

  public canWrite(roleType: RoleType): boolean | undefined {
    return this.isPermited(roleType, Operations.Write);
  }

  public canExecute(methodType: MethodType): boolean | undefined {
    return this.isPermited(methodType, Operations.Execute);
  }

  public isPermited(operandType: OperandType, operation: Operations): boolean | undefined {
    if (this.roleByRoleType === undefined) {
      return undefined;
    }

    if (this.newId) {
      return true;
    } else if (this.databaseObject) {
      const permission = this.workspace.database.permission(this.objectType, operandType, operation);
      return permission ? this.databaseObject.isPermitted(permission) : false;
    }

    return false;
  }

  public method(methodType: MethodType): Method | undefined {
    if (this.roleByRoleType === undefined) {
      return undefined;
    }

    return new Method(this, methodType);
  }

  public get(roleType: RoleType): any {
    if (this.roleByRoleType === undefined) {
      return undefined;
    }

    let value = this.roleByRoleType.get(roleType);
    if (value === undefined) {
      if (this.newId === undefined) {
        if (roleType.objectType.isUnit) {
          value = this.databaseObject?.roleByRoleTypeId.get(roleType.id);
          if (value === undefined) {
            value = null;
          }
        } else {
          try {
            if (roleType.isOne) {
              const role: string = this.databaseObject?.roleByRoleTypeId.get(roleType.id);
              value = role ? this.workspace.get(role) : null;
            } else {
              const roles: string[] = this.databaseObject?.roleByRoleTypeId.get(roleType.id);
              value = roles
                ? roles.map((role) => {
                    return this.workspace.get(role);
                  })
                : [];
            }
          } catch (e) {
            let stringValue = 'N/A';
            try {
              stringValue = this.toString();
            } catch (e2) {
              throw new Error(`Could not get role ${roleType.name} from [objectType: ${this.objectType.name}, id: ${this.id}]`);
            }

            throw new Error(
              `Could not get role ${roleType.name} from [objectType: ${this.objectType.name}, id: ${this.id}, value: '${stringValue}']`
            );
          }
        }
      } else {
        if (roleType.objectType.isComposite && roleType.isMany) {
          value = [];
        } else {
          value = null;
        }
      }

      this.roleByRoleType.set(roleType, value);
    }

    return value;
  }

  public getForAssociation(roleType: RoleType): any {
    if (this.roleByRoleType === undefined) {
      return undefined;
    }

    let value = this.roleByRoleType.get(roleType);
    if (value === undefined) {
      if (this.newId === undefined) {
        if (roleType.objectType.isUnit) {
          value = this.databaseObject?.roleByRoleTypeId.get(roleType.id);
          if (value === undefined) {
            value = null;
          }
        } else {
          if (roleType.isOne) {
            const role: string = this.databaseObject?.roleByRoleTypeId.get(roleType.id);
            value = role ? this.workspace.getForAssociation(role) : null;
          } else {
            const roles: string[] = this.databaseObject?.roleByRoleTypeId.get(roleType.id);
            value = roles
              ? roles.map((role) => {
                  return this.workspace.getForAssociation(role);
                })
              : [];
          }
        }
      } else {
        if (roleType.objectType.isComposite && roleType.isMany) {
          value = [];
        } else {
          value = null;
        }
      }

      this.roleByRoleType.set(roleType, value);
    }

    return value;
  }

  public set(roleType: RoleType, value: any) {
    this.assertExists();

    if (this.changedRoleByRoleType === undefined) {
      this.changedRoleByRoleType = new Map();
    }

    if (value === undefined) {
      value = null;
    }

    if (value === null) {
      if (roleType.objectType.isComposite && roleType.isMany) {
        value = [];
      }
    }

    if (value === '') {
      if (roleType.objectType.isUnit) {
        if (!roleType.objectType.isString) {
          value = null;
        }
      }
    }

    this.roleByRoleType!.set(roleType, value);
    this.changedRoleByRoleType.set(roleType, value);

    // TODO: ChangeSet
  }

  public add(roleType: RoleType, value: WorkspaceObject) {
    if (value) {
      this.assertExists();

      const roles = this.get(roleType);
      if (roles.indexOf(value) < 0) {
        roles.push(value);
      }

      this.set(roleType, roles);

       // TODO: ChangeSet
    }
  }

  public remove(roleType: RoleType, value: WorkspaceObject) {
    if (value) {
      this.assertExists();

      const roles = this.get(roleType) as [];
      const newRoles = roles.filter((v) => v !== value);

      this.set(roleType, newRoles);

       // TODO: ChangeSet
    }
  }

  public getAssociation(associationType: AssociationType): any {
    this.assertExists();

    const associations = this.workspace.getAssociation(this, associationType);

    if (associationType.isOne) {
      const association = associations.length > 0 ? associations[0] : null;
      const roleType = associationType.relationType.roleType;

      if (association) {
        if (roleType.isOne && association.get(roleType) === this) {
          return association;
        }

        if (roleType.isMany && association.get(roleType).indexOf(this) > -1) {
          return association;
        }
      }

      return null;
    }

    return associations;
  }

  public save(): PushRequestObject | undefined {
    if (this.changedRoleByRoleType !== undefined) {
      const data: PushRequestObject = {
        i: this.id,
        v: this.version,
        roles: this.saveRoles(),
      };

      return data;
    }

    return undefined;
  }

  public saveNew(): PushRequestNewObject {
    this.assertExists();

    const data: PushRequestNewObject = {
      ni: this.newId!,
      t: this.objectType.id,
    };

    if (this.changedRoleByRoleType !== undefined) {
      data.roles = this.saveRoles();
    }

    return data;
  }

  public reset() {
    if (this.newId) {
      delete this.newId;
      delete this.workspace;
      delete this.objectType;
      delete this.roleByRoleType;
    } else {
      this.databaseObject = this.databaseObject?.database.get(this.id) ?? undefined;
      this.roleByRoleType = new Map();
    }

    delete this.changedRoleByRoleType;
  }

  public onDelete(deleted: WorkspaceObject) {
    if (this.changedRoleByRoleType !== undefined) {
      for (const [roleType, value] of this.changedRoleByRoleType) {
        if (!roleType.objectType.isUnit) {
          if (roleType.isOne) {
            const role = value as WorkspaceObject;
            if (role && role === deleted) {
              this.set(roleType, null);
            }
          } else {
            const roles = value as WorkspaceObject[];
            if (roles && roles.indexOf(deleted) > -1) {
              this.remove(roleType, deleted);
            }
          }
        }
      }
    }
  }

  protected init() {
    this.roleByRoleType = new Map();
  }

  private assertExists() {
    if (this.roleByRoleType === undefined) {
      throw new Error("Object doesn't exist anymore.");
    }
  }

  private saveRoles(): PushRequestRole[] | undefined {
    if (this.changedRoleByRoleType) {
      const saveRoles = new Array<PushRequestRole>();

      for (const [roleType, value] of this.changedRoleByRoleType) {
        const role = value;
        const saveRole: PushRequestRole = {
          t: roleType.id,
        };

        if (roleType.objectType.isUnit) {
          saveRole.s = serialize(role);
        } else {
          if (roleType.isOne) {
            saveRole.s = role ? role.id || role.newId : null;
          } else {
            const roleIds = role.map((item: WorkspaceObject) => item.id ?? item.newId);
            if (this.newId) {
              saveRole.a = roleIds;
            } else {
              const originalRoleIds = this.databaseObject?.roleByRoleTypeId.get(roleType.id) as string[];
              if (!originalRoleIds) {
                saveRole.a = roleIds;
              } else {
                saveRole.a = roleIds.filter((v: string) => originalRoleIds.indexOf(v) < 0);
                saveRole.r = originalRoleIds.filter((v) => roleIds.indexOf(v) < 0);
              }
            }
          }
        }

        saveRoles.push(saveRole);
      }

      return saveRoles;
    }

    return undefined;
  }
}
