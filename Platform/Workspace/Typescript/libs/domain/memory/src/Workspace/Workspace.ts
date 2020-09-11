import { ObjectType, AssociationType } from '@allors/meta/system';
import { Operations, PushRequestObject, PushRequest, PushResponse } from '@allors/protocol/system';
import { DatabaseObject, Workspace, WorkspaceObject } from '@allors/domain/system';

import { MemoryDatabase } from '../Database/Database';

import { MemoryWorkspaceObject } from './WorkspaceObject';
import { Origin } from '@allors/meta/system';

export class MemoryWorkspace implements Workspace {
  private static idCounter = 0;

  private workspaceObjectById: Map<string, MemoryWorkspaceObject>;
  private workspaceObjectByIdByClass: Map<ObjectType, Map<string, MemoryWorkspaceObject>>;

  constructor(public database: MemoryDatabase) {
    this.workspaceObjectById = new Map();
    this.workspaceObjectByIdByClass = new Map();
  }

  public get(id: string): WorkspaceObject | undefined {
    if (!id) {
      return undefined;
    }

    let workspaceObject = this.workspaceObjectById.get(id);
    if (workspaceObject === undefined) {
      const databaseObject = this.database.get(id);
      if (databaseObject) {
        workspaceObject = this.instantiate(databaseObject);
      }
    }

    return workspaceObject;
  }

  public getForAssociation(id: string): WorkspaceObject | undefined {
    if (!id) {
      return undefined;
    }

    let workspaceObject = this.workspaceObjectById.get(id);
    if (!workspaceObject) {
      const databaseObject = this.database.getForAssociation(id);

      if (databaseObject) {
        workspaceObject = this.instantiate(databaseObject);
      }
    }

    return workspaceObject;
  }

  public create(objectType: ObjectType | string): WorkspaceObject {
    const resolvedObjectType = typeof objectType === 'string' ? this.database.metaPopulation.objectTypeByName.get(objectType) : objectType;

    if (!resolvedObjectType) {
      throw new Error(`Could not find class for ${objectType}`);
    }

    if (resolvedObjectType.origin != Origin.Workspace) {
      throw new Error(`${objectType} is not a Workspace Type`);
    }

    const constructor = this.database.workspaceConstructorByObjectType.get(resolvedObjectType) as any;
    if (!constructor) {
      throw new Error(`Could not get constructor for ${resolvedObjectType.name}`);
    }

    const newWorkspaceObject: MemoryWorkspaceObject = new constructor();
    newWorkspaceObject.workspace = this;
    newWorkspaceObject.objectType = resolvedObjectType;
    newWorkspaceObject.newId = 'w' + MemoryWorkspace.idCounter.toString();

    this.workspaceObjectById.set(newWorkspaceObject.newId, newWorkspaceObject);
    this.addByObjectTypeId(newWorkspaceObject);

    return newWorkspaceObject;
  }

  public delete(object: WorkspaceObject): void {
    if (object.objectType.origin != Origin.Workspace) {
      throw new Error('Only workspace objects can be deleted');
    }

    // TODO:
  }

  public getAssociation(object: MemoryWorkspaceObject, associationType: AssociationType): WorkspaceObject[] {
    const associationClasses = associationType.objectType.classes;
    const roleType = associationType.relationType.roleType;

    const associationIds = new Set<string>();
    const associations: WorkspaceObject[] = [];

    associationClasses.forEach((associationClass) => {
      this.getAll(associationClass);
      const workspaceObjectById = this.workspaceObjectByIdByClass.get(associationClass);
      if (workspaceObjectById) {
        for (const association of workspaceObjectById.values()) {
          if (!associationIds.has(association.id) && association.canRead(roleType)) {
            if (roleType.isOne) {
              const role: WorkspaceObject = association.getForAssociation(roleType);
              if (role && role.id === object.id) {
                associationIds.add(association.id);
                associations.push(association);
              }
            } else {
              const roles: WorkspaceObject[] = association.getForAssociation(roleType);
              if (roles && roles.find((v) => v === object)) {
                associationIds.add(association.id);
                associations.push(association);
              }
            }
          }
        }
      }
    });

    if (associationType.isOne && associations.length > 0) {
      return associations;
    }

    associationClasses.forEach((associationClass) => {
      const databaseObjects = this.database.databaseObjectsByClass.get(associationClass);
      if (databaseObjects) {
        for (const databaseObject of databaseObjects) {
          if (!associationIds.has(databaseObject.id)) {
            const permission = this.database.permission(databaseObject.objectType, roleType, Operations.Read);
            if (permission && databaseObject.isPermitted(permission)) {
              if (roleType.isOne) {
                const role: string = databaseObject.roleByRoleTypeId.get(roleType.id);
                if (object.id === role) {
                  associations.push(this.get(databaseObject.id) as WorkspaceObject);
                  break;
                }
              } else {
                const roles: string[] = databaseObject.roleByRoleTypeId.get(roleType.id);
                if (roles && roles.indexOf(databaseObject.id) > -1) {
                  associationIds.add(databaseObject.id);
                  associations.push(this.get(databaseObject.id) as WorkspaceObject);
                }
              }
            }
          }
        }
      }
    });

    return associations;
  }

  private instantiate(databaseObject: DatabaseObject): MemoryWorkspaceObject {
    const constructor = this.database.workspaceConstructorByObjectType.get(databaseObject.objectType) as any;
    if (!constructor) {
      throw new Error(`Could not get constructor for ${databaseObject.objectType.name}`);
    }

    const workspaceObject: MemoryWorkspaceObject = new constructor();
    workspaceObject.workspace = this;
    workspaceObject.databaseObject = databaseObject;
    workspaceObject.objectType = databaseObject.objectType;

    this.workspaceObjectById.set(workspaceObject.id, workspaceObject);
    this.addByObjectTypeId(workspaceObject);

    return workspaceObject;
  }

  private getAll(objectType: ObjectType): void {
    const databaseObjects = this.database.databaseObjectsByClass.get(objectType);
    if (databaseObjects) {
      for (const databaseObject of databaseObjects) {
        this.get(databaseObject.id);
      }
    }
  }

  private addByObjectTypeId(workspaceObject: MemoryWorkspaceObject) {
    let workspaceObjectById = this.workspaceObjectByIdByClass.get(workspaceObject.objectType);
    if (!workspaceObjectById) {
      workspaceObjectById = new Map();
      this.workspaceObjectByIdByClass.set(workspaceObject.objectType, workspaceObjectById);
    }

    workspaceObjectById.set(workspaceObject.id, workspaceObject);
  }

  private removeByObjectTypeId(objectType: ObjectType, id: string) {
    const workspaceObjectById = this.workspaceObjectByIdByClass.get(objectType);
    if (workspaceObjectById) {
      workspaceObjectById.delete(id);
    }
  }
}
