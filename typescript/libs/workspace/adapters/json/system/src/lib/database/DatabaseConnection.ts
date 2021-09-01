import { PullResponse, SecurityRequest, SecurityResponse, SyncRequest, SyncResponse } from '@allors/protocol/json/system';
import { AccessControl, Configuration, DatabaseConnection as SystemDatabaseConnection, IdGenerator, MapMap, ServicesBuilder } from '@allors/workspace/adapters/system';
import { IWorkspace, Operations } from '@allors/workspace/domain/system';
import { Class, MethodType, OperandType, RelationType } from '@allors/workspace/meta/system';
import { DatabaseRecord } from './DatabaseRecord';
import { ResponseContext } from './Security/ResponseContext';
import { Workspace } from '../workspace/Workspace';

export class DatabaseConnection extends SystemDatabaseConnection {
  private recordsById: Map<number, DatabaseRecord>;

  accessControlById: Map<number, AccessControl>;
  permissions: Set<number>;
  
  readPermissionByOperandTypeByClass: MapMap<Class, OperandType, number>;
  writePermissionByOperandTypeByClass: MapMap<Class, OperandType, number>;
  executePermissionByOperandTypeByClass: MapMap<Class, OperandType, number>;

  constructor(configuration: Configuration, idGenerator: IdGenerator, private servicesBuilder: ServicesBuilder) {
    super(configuration, idGenerator);

    this.recordsById = new Map();

    this.accessControlById = new Map();
    this.permissions = new Set();

    this.readPermissionByOperandTypeByClass = new MapMap();
    this.writePermissionByOperandTypeByClass = new MapMap();
    this.executePermissionByOperandTypeByClass = new MapMap();
  }

  createWorkspace(): IWorkspace {
    return new Workspace(this, this.servicesBuilder());
  }

  onPullResonse(response: PullResponse): SyncRequest {
    return {
      o: response.p
        .filter((v) => {
          const record = this.recordsById.get(v.i);

          if (record == null) {
            return true;
          }

          if (record.version !== v.v) {
            return true;
          }

          if (!this.ranges.equals(record.accessControlIds, v.a)) {
            return true;
          }

          if (!this.ranges.equals(record.deniedPermissionIds, v.d)) {
            return true;
          }

          // TODO: Use smarter updates for DeniedPermissions

          return false;
        })
        .map((v) => v.i),
    };
  }

  onSyncResponse(syncResponse: SyncResponse): SecurityRequest | null {
    const ctx = new ResponseContext(this);

    for (const syncResponseObject of syncResponse.o) {
      const databaseObjects = DatabaseRecord.fromResponse(this, ctx, syncResponseObject);
      this.recordsById.set(databaseObjects.id, databaseObjects);
    }

    if (ctx.missingAccessControlIds.size > 0 || ctx.missingPermissionIds.size > 0) {
      return {
        a: Array.from(ctx.missingAccessControlIds),
        p: Array.from(ctx.missingPermissionIds),
      };
    }

    return null;
  }

  securityResponse(securityResponse: SecurityResponse): SecurityRequest | undefined {
    if (securityResponse.p != null) {
      for (const syncResponsePermission of securityResponse.p) {
        const id = syncResponsePermission[0];
        const cls = this.configuration.metaPopulation.metaObjectByTag.get(syncResponsePermission[1]) as Class;
        const metaObject = this.configuration.metaPopulation.metaObjectByTag.get(syncResponsePermission[2]);
        const operandType: OperandType = (metaObject as RelationType)?.roleType ?? (metaObject as MethodType);
        const operation = syncResponsePermission[3];

        this.permissions.add(id);

        switch (operation) {
          case Operations.Read:
            this.readPermissionByOperandTypeByClass.set(cls, operandType, id);
            break;
          case Operations.Write:
            this.writePermissionByOperandTypeByClass.set(cls, operandType, id);
            break;
          case Operations.Execute:
            this.executePermissionByOperandTypeByClass.set(cls, operandType, id);
            break;
          case Operations.Create:
            throw new Error('Create is not supported');
          default:
            throw new Error('Argument out of range');
        }
      }
    }

    let missingPermissionIds: Set<number> | undefined = undefined;
    if (securityResponse.a != null) {
      for (const syncResponseAccessControl of securityResponse.a) {
        const id = syncResponseAccessControl.i;
        const version = syncResponseAccessControl.v;
        const permissionIds = syncResponseAccessControl.p;
        this.accessControlById.set(id, new AccessControl(version, permissionIds));

        if (permissionIds != null) {
          for (const permissionId of permissionIds) {
            if (this.permissions.has(permissionId)) {
              continue;
            }

            missingPermissionIds ??= new Set();
            missingPermissionIds.add(permissionId);
          }
        }
      }
    }

    if (missingPermissionIds != null) {
      return {
        p: Array.from(missingPermissionIds),
      };
    }

    return undefined;
  }

  getPermission(cls: Class, operandType: OperandType, operation: Operations): number | undefined {
    switch (operation) {
      case Operations.Read:
        return this.readPermissionByOperandTypeByClass.get(cls, operandType);
      case Operations.Write:
        return this.writePermissionByOperandTypeByClass.get(cls, operandType);
      case Operations.Execute:
        return this.executePermissionByOperandTypeByClass.get(cls, operandType);
      case Operations.Create:
        throw new Error('Create is not supported');
      default:
        throw new Error('Argument out of range');
    }
  }

  getRecord(id: number): DatabaseRecord | undefined {
    return this.recordsById.get(id);
  }
}
