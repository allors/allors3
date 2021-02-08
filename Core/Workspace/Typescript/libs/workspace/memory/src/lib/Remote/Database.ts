import { ObjectType, MetaPopulation, OperandType, Origin, RelationType, MethodType } from '@allors/meta/core';
import {
  Operations,
  Compressor,
  PullResponse,
  SyncRequest,
  SyncResponse,
  SecurityRequest,
  SecurityResponse,
} from '@allors/protocol/core';
import {
  Permission,
  ReadPermission,
  WritePermission,
  ExecutePermission,
  Database,
  Record,
  Session,
  AccessControl,
} from '@allors/workspace/core';

import { MemorySession } from '../Working/Session';
import { MemoryWorkspace } from '../Local/Workspace';

import { databaseClasses } from '../Working/databaseClasses';
import { workspaceClasses } from '../Working/workspaceClasses';

import { MemoryRecord } from './Record';

export class MemoryDatabase implements Database {
  constructorByObjectType: Map<ObjectType, any>;

  readonly workspace: MemoryWorkspace;

  readonly databaseObjectById: Map<string, MemoryRecord>;
  readonly databaseObjectsByClass: Map<ObjectType, Set<MemoryRecord>>;

  readonly accessControlById: Map<string, AccessControl>;
  readonly permissionById: Map<string, Permission>;

  private readPermissionByOperandTypeByClass: Map<ObjectType, Map<OperandType, ReadPermission>>;
  private writePermissionByOperandTypeByClass: Map<ObjectType, Map<OperandType, WritePermission>>;
  private executePermissionByOperandTypeByClass: Map<ObjectType, Map<OperandType, ExecutePermission>>;

  constructor(public metaPopulation: MetaPopulation) {
    this.constructorByObjectType = new Map();

    this.databaseObjectById = new Map();
    this.databaseObjectsByClass = new Map();
    for (const objectType of metaPopulation.classes) {
      this.databaseObjectsByClass.set(objectType, new Set());
    }

    this.accessControlById = new Map();
    this.permissionById = new Map();

    this.readPermissionByOperandTypeByClass = new Map();
    this.writePermissionByOperandTypeByClass = new Map();
    this.executePermissionByOperandTypeByClass = new Map();

    metaPopulation.classes.forEach((v) => {
      this.readPermissionByOperandTypeByClass.set(v, new Map());
      this.writePermissionByOperandTypeByClass.set(v, new Map());
      this.executePermissionByOperandTypeByClass.set(v, new Map());
    });

    const classes = metaPopulation.classes;
    databaseClasses(
      classes.filter((v) => v.origin === Origin.Remote),
      this.constructorByObjectType
    );
    workspaceClasses(
      classes.filter((v) => v.origin === Origin.Local),
      this.constructorByObjectType
    );

    this.workspace = new MemoryWorkspace(metaPopulation);
  }

  createSession(): Session {
    return new MemorySession(this);
  }

  get(id: string): Record | undefined {
    const databaseObject = this.databaseObjectById.get(id);
    if (databaseObject === undefined) {
      throw new Error(`Object with id ${id} is not present.`);
    }

    return databaseObject ?? null;
  }

  getForAssociation(id: string): Record | undefined {
    return this.databaseObjectById.get(id);
  }

  diff(response: PullResponse): SyncRequest {
    const syncRequest: SyncRequest = {
      objects: (response.objects ?? [])
        .filter((syncRequestObject) => {
          const [id, version, sortedAccessControlIds, sortedDeniedPermissionIds] = syncRequestObject;
          const databaseObject = this.databaseObjectById.get(id);

          const sync =
            databaseObject == null ||
            databaseObject.version !== version ||
            databaseObject.sortedAccessControlIds !== sortedAccessControlIds ||
            databaseObject.sortedDeniedPermissionIds !== sortedDeniedPermissionIds;

          return sync;
        })
        .map((syncRequestObject) => {
          return syncRequestObject[0];
        }),
    };

    return syncRequest;
  }

  sync(syncResponse: SyncResponse): SecurityRequest | undefined {
    const missingAccessControlIds = new Set<string>();
    const missingPermissionIds = new Set<string>();

    const sortedAccessControlIdsDecompress = (compressed: string): string => {
      if (compressed) {
        compressed.split(Compressor.itemSeparator).forEach((v) => {
          if (!this.accessControlById.has(v)) {
            missingAccessControlIds.add(v);
          }
        });
      }

      return compressed;
    };

    const sortedDeniedPermissionIdsDecompress = (compressed: string): string => {
      if (compressed) {
        compressed.split(Compressor.itemSeparator).forEach((v) => {
          if (!this.permissionById.has(v)) {
            missingPermissionIds.add(v);
          }
        });
      }

      return compressed;
    };

    if (syncResponse.objects) {
      syncResponse.objects.forEach((v) => {
        const databaseObject = new MemoryRecord(
          this,
          this.metaPopulation,
          v,
          sortedAccessControlIdsDecompress,
          sortedDeniedPermissionIdsDecompress
        );

        this.add(databaseObject);
      });
    }

    if (missingAccessControlIds.size > 0 || missingPermissionIds.size > 0) {
      const securityRequest: SecurityRequest = {
        accessControls: [...missingAccessControlIds],
        permissions: [...missingPermissionIds],
      };

      return securityRequest;
    }

    return undefined;
  }

  private getOrCreate<T, U, V>(map: Map<T, Map<U, V>>, key: T) {
    let value = map.get(key);
    if (!value) {
      value = new Map();
      map.set(key, value);
    }

    return value;
  }

  security(securityResponse: SecurityResponse): SecurityRequest | undefined {
    if (securityResponse.permissions) {
      securityResponse.permissions.forEach((v) => {
        const id = v[0];
        const objectType = this.metaPopulation.metaObjectById.get(v[1]) as ObjectType;
        const operandType = this.metaPopulation.metaObjectById.get(v[2]) as RelationType | MethodType;
        const operation = parseInt(v[3], 10);

        if (!this.permissionById.has(id)) {
          switch (operation) {
            case Operations.Read:
              {
                const permission = new ReadPermission(id, objectType, (operandType as RelationType).roleType);
                this.permissionById.set(id, permission);
                this.getOrCreate(this.readPermissionByOperandTypeByClass, objectType).set(permission.roleType, permission);
              }
              break;

            case Operations.Write:
              {
                const permission = new WritePermission(id, objectType, (operandType as RelationType).roleType);
                this.permissionById.set(id, permission);
                this.getOrCreate(this.writePermissionByOperandTypeByClass, objectType).set(permission.roleType, permission);
              }
              break;

            case Operations.Execute:
              {
                const permission = new ExecutePermission(id, objectType, operandType as MethodType);
                this.permissionById.set(id, permission);
                this.getOrCreate(this.executePermissionByOperandTypeByClass, objectType).set(permission.methodType, permission);
              }
              break;
          }
        }
      });
    }

    if (securityResponse.accessControls) {
      let missingPermissionIds: Set<string> = new Set();

      securityResponse.accessControls.forEach((v) => {
        const id = v.i;
        const version = v.v;
        const permissionIds = new Set<string>();
        v.p?.split(',').forEach((w) => {
          if (!this.permissionById.has(w)) {
            if (missingPermissionIds === undefined) {
              missingPermissionIds = new Set();
            }
            missingPermissionIds.add(w);
          }
          permissionIds.add(w);
        });
        const accessControl = new AccessControl(id, version, permissionIds);
        this.accessControlById.set(id, accessControl);
      });

      if (missingPermissionIds.size > 0) {
        const securityRequest: SecurityRequest = {
          permissions: [...missingPermissionIds],
        };

        return securityRequest;
      }
    }

    return undefined;
  }

  new(id: string, objectType: ObjectType): Record {
    const databaseObject = new MemoryRecord(this, objectType, id);
    this.add(databaseObject);
    return databaseObject;
  }

  permission(objectType: ObjectType, operandType: OperandType, operation: Operations): Permission | undefined {
    switch (operation) {
      case Operations.Read:
        return this.readPermissionByOperandTypeByClass.get(objectType)?.get(operandType);
      case Operations.Write:
        return this.writePermissionByOperandTypeByClass.get(objectType)?.get(operandType);
      default:
        return this.executePermissionByOperandTypeByClass.get(objectType)?.get(operandType);
    }
  }

  private add(databaseObject: MemoryRecord) {
    this.databaseObjectById.set(databaseObject.id, databaseObject);
    this.databaseObjectsByClass.get(databaseObject.objectType)?.add(databaseObject);
  }
}
