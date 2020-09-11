import { ObjectType, AssociationType } from '@allors/meta/system';
import { Operations, PushRequestObject, PushRequest, PushResponse } from '@allors/protocol/system';
import { DatabaseObject, Session, SessionObject } from '@allors/domain/system';

import { MemoryDatabase } from '../Database/Database';

import { MemorySessionObject } from './SessionObject';

export class MemorySession implements Session {
  private static idCounter = 0;

  hasChanges: boolean;

  private existingSessionObjectById: Map<string, MemorySessionObject>;
  private newSessionObjectById: Map<string, MemorySessionObject>;

  private sessionObjectByIdByClass: Map<ObjectType, Map<string, MemorySessionObject>>;

  constructor(public database: MemoryDatabase) {
    this.hasChanges = false;

    this.existingSessionObjectById = new Map();
    this.newSessionObjectById = new Map();

    this.sessionObjectByIdByClass = new Map();
  }

  public get(id: string): SessionObject | undefined {
    if (!id) {
      return undefined;
    }

    let sessionObject = this.existingSessionObjectById.get(id);
    if (sessionObject === undefined) {
      sessionObject = this.newSessionObjectById.get(id);

      if (sessionObject === undefined) {
        const databaseObject = this.database.get(id);
        if (databaseObject) {
          sessionObject = this.instantiate(databaseObject);
        }
      }
    }

    return sessionObject;
  }

  public getForAssociation(id: string): SessionObject | undefined {
    if (!id) {
      return undefined;
    }

    let sessionObject = this.existingSessionObjectById.get(id);
    if (!sessionObject) {
      sessionObject = this.newSessionObjectById.get(id);

      if (!sessionObject) {
        const databaseObject = this.database.getForAssociation(id);

        if (databaseObject) {
          sessionObject = this.instantiate(databaseObject);
        }
      }
    }

    return sessionObject;
  }

  public create(objectType: ObjectType | string): SessionObject {
    const resolvedObjectType = typeof objectType === 'string' ? this.database.metaPopulation.objectTypeByName.get(objectType) : objectType;

    if (!resolvedObjectType) {
      throw new Error(`Could not find class for ${objectType}`);
    }

    const constructor = this.database.sessionConstructorByObjectType.get(resolvedObjectType) as any;
    if (!constructor) {
      throw new Error(`Could not get constructor for ${resolvedObjectType.name}`);
    }

    const newSessionObject: MemorySessionObject = new constructor();
    newSessionObject.session = this;
    newSessionObject.objectType = resolvedObjectType;
    newSessionObject.newId = (--MemorySession.idCounter).toString();

    this.newSessionObjectById.set(newSessionObject.newId, newSessionObject);
    this.addByObjectTypeId(newSessionObject);

    this.hasChanges = true;

    return newSessionObject;
  }

  public delete(object: SessionObject): void {
    if (!object.isNew) {
      throw new Error('Existing objects can not be deleted');
    }

    const newSessionObject = object as SessionObject;
    const newId = newSessionObject.newId!;

    if (this.newSessionObjectById.has(newId)) {
      for (const sessionObject of this.newSessionObjectById.values()) {
        sessionObject.onDelete(newSessionObject);
      }

      for (const sessionObject of this.existingSessionObjectById.values()) {
        sessionObject.onDelete(newSessionObject);
      }

      const objectType = newSessionObject.objectType;
      newSessionObject.reset();

      this.newSessionObjectById.delete(newId);
      this.removeByObjectTypeId(objectType, newId);
    }
  }

  public reset(): void {
    for (const sessionObject of this.newSessionObjectById.values()) {
      sessionObject.reset();
    }

    for (const sessionObject of this.existingSessionObjectById.values()) {
      sessionObject.reset();
    }

    this.hasChanges = false;
  }

  public pushRequest(): PushRequest {
    const newObjects = Array.from(this.newSessionObjectById.values()).map((v) => v.saveNew());
    const objects = Array.from(this.existingSessionObjectById.values())
      .map((v) => v.save())
      .filter((v) => v) as PushRequestObject[];

    return new PushRequest({
      newObjects,
      objects,
    });
  }

  public pushResponse(pushResponse: PushResponse): void {
    if (pushResponse.newObjects) {
      pushResponse.newObjects.forEach((pushResponseNewObject) => {
        const newId = pushResponseNewObject.ni;
        const id = pushResponseNewObject.i;

        const sessionObject = this.newSessionObjectById.get(newId);
        if (sessionObject) {
          delete sessionObject.newId;
          sessionObject.databaseObject = this.database.new(id, sessionObject.objectType);

          this.newSessionObjectById.delete(newId);
          this.existingSessionObjectById.set(id, sessionObject);

          this.removeByObjectTypeId(sessionObject.objectType, newId);
          this.addByObjectTypeId(sessionObject);
        }
      });
    }

    if (Object.getOwnPropertyNames(this.newSessionObjectById).length !== 0) {
      throw new Error('Not all new objects received ids');
    }
  }

  public getAssociation(object: MemorySessionObject, associationType: AssociationType): SessionObject[] {
    const associationClasses = associationType.objectType.classes;
    const roleType = associationType.relationType.roleType;

    const associationIds = new Set<string>();
    const associations: SessionObject[] = [];

    associationClasses.forEach((associationClass) => {
      this.getAll(associationClass);
      const sessionObjectById = this.sessionObjectByIdByClass.get(associationClass);
      if (sessionObjectById) {
        for (const association of sessionObjectById.values()) {
          if (!associationIds.has(association.id) && association.canRead(roleType)) {
            if (roleType.isOne) {
              const role: SessionObject = association.getForAssociation(roleType);
              if (role && role.id === object.id) {
                associationIds.add(association.id);
                associations.push(association);
              }
            } else {
              const roles: SessionObject[] = association.getForAssociation(roleType);
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
                  associations.push(this.get(databaseObject.id) as SessionObject);
                  break;
                }
              } else {
                const roles: string[] = databaseObject.roleByRoleTypeId.get(roleType.id);
                if (roles && roles.indexOf(databaseObject.id) > -1) {
                  associationIds.add(databaseObject.id);
                  associations.push(this.get(databaseObject.id) as SessionObject);
                }
              }
            }
          }
        }
      }
    });

    return associations;
  }

  private instantiate(databaseObject: DatabaseObject): MemorySessionObject {
    const constructor = this.database.sessionConstructorByObjectType.get(databaseObject.objectType) as any;
    if (!constructor) {
      throw new Error(`Could not get constructor for ${databaseObject.objectType.name}`);
    }

    const sessionObject: MemorySessionObject = new constructor();
    sessionObject.session = this;
    sessionObject.databaseObject = databaseObject;
    sessionObject.objectType = databaseObject.objectType;

    this.existingSessionObjectById.set(sessionObject.id, sessionObject);
    this.addByObjectTypeId(sessionObject);

    return sessionObject;
  }

  private getAll(objectType: ObjectType): void {
    const databaseObjects = this.database.databaseObjectsByClass.get(objectType);
    if (databaseObjects) {
      for (const databaseObject of databaseObjects) {
        this.get(databaseObject.id);
      }
    }
  }

  private addByObjectTypeId(sessionObject: MemorySessionObject) {
    let sessionObjectById = this.sessionObjectByIdByClass.get(sessionObject.objectType);
    if (!sessionObjectById) {
      sessionObjectById = new Map();
      this.sessionObjectByIdByClass.set(sessionObject.objectType, sessionObjectById);
    }

    sessionObjectById.set(sessionObject.id, sessionObject);
  }

  private removeByObjectTypeId(objectType: ObjectType, id: string) {
    const sessionObjectById = this.sessionObjectByIdByClass.get(objectType);
    if (sessionObjectById) {
      sessionObjectById.delete(id);
    }
  }
}
