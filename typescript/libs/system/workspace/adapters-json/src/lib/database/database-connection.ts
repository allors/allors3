import {
  PullResponse,
  SyncRequest,
  SyncResponse,
  AccessRequest,
  AccessResponse,
  PermissionRequest,
  PermissionResponse,
} from '@allors/system/common/protocol-json';
import {
  Grant,
  DatabaseConnection as SystemDatabaseConnection,
  MapMap,
  Revocation,
} from '@allors/system/workspace/adapters';
import {
  Configuration,
  IWorkspace,
  Operations,
} from '@allors/system/workspace/domain';
import {
  Class,
  MethodType,
  OperandType,
  RelationType,
} from '@allors/system/workspace/meta';
import { DatabaseRecord } from './database-record';
import { ResponseContext } from './security/response-context';
import { Workspace } from '../workspace/workspace';
import { IDatabaseJsonClient } from '../idatabase-json-client';

export class DatabaseConnection extends SystemDatabaseConnection {
  private recordsById: Map<number, DatabaseRecord>;

  grantById: Map<number, Grant>;
  revocationById: Map<number, Revocation>;
  permissions: Set<number>;

  readPermissionByOperandTypeByClass: MapMap<Class, OperandType, number>;
  writePermissionByOperandTypeByClass: MapMap<Class, OperandType, number>;
  executePermissionByOperandTypeByClass: MapMap<Class, OperandType, number>;

  constructor(
    configuration: Configuration,
    public client: IDatabaseJsonClient
  ) {
    super(configuration);

    this.recordsById = new Map();

    this.grantById = new Map();
    this.revocationById = new Map();
    this.permissions = new Set();

    this.readPermissionByOperandTypeByClass = new MapMap();
    this.writePermissionByOperandTypeByClass = new MapMap();
    this.executePermissionByOperandTypeByClass = new MapMap();
  }

  createWorkspace(): IWorkspace {
    return new Workspace(this);
  }

  onPullResonse(response: PullResponse, context: string): SyncRequest {
    return {
      x: context,
      o: response.p
        .filter((v) => {
          const record = this.recordsById.get(v.i);

          if (record == null) {
            return true;
          }

          if (record.version !== v.v) {
            return true;
          }

          if (!this.ranges.equals(record.grants, v.g)) {
            return true;
          }

          if (!this.ranges.equals(record.revocations, v.r)) {
            return true;
          }

          return false;
        })
        .map((v) => v.i),
    };
  }

  onSyncResponse(syncResponse: SyncResponse): AccessRequest | null {
    const ctx = new ResponseContext(this);

    for (const syncResponseObject of syncResponse.o) {
      const databaseObjects = DatabaseRecord.fromResponse(
        this,
        ctx,
        syncResponseObject
      );
      this.recordsById.set(databaseObjects.id, databaseObjects);
    }

    if (ctx.missingGrantIds.size > 0 || ctx.missingRevocationIds.size > 0) {
      return {
        g: Array.from(ctx.missingGrantIds),
        r: Array.from(ctx.missingRevocationIds),
      };
    }

    return null;
  }

  accessResponse(
    accessResponse: AccessResponse
  ): PermissionRequest | undefined {
    let missingPermissionIds: Set<number> | undefined = undefined;

    if (accessResponse.g != null) {
      for (const accessResponseGrant of accessResponse.g) {
        const id = accessResponseGrant.i;
        const version = accessResponseGrant.v;
        const permissionIds = accessResponseGrant.p;
        this.grantById.set(id, new Grant(version, permissionIds));

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

    if (accessResponse.r != null) {
      for (const accessResponseRevocation of accessResponse.r) {
        const id = accessResponseRevocation.i;
        const version = accessResponseRevocation.v;
        const permissionIds = accessResponseRevocation.p;
        this.revocationById.set(id, new Revocation(version, permissionIds));

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

  permissionResponse(permissionResponse: PermissionResponse): void {
    if (permissionResponse.p != null) {
      for (const syncResponsePermission of permissionResponse.p) {
        const id = syncResponsePermission.i;
        const cls = this.configuration.metaPopulation.metaObjectByTag.get(
          syncResponsePermission.c
        ) as Class;
        const metaObject =
          this.configuration.metaPopulation.metaObjectByTag.get(
            syncResponsePermission.t
          );
        const operandType: OperandType =
          (metaObject as RelationType)?.roleType ?? (metaObject as MethodType);
        const operation = syncResponsePermission.o;

        this.permissions.add(id);

        switch (operation) {
          case Operations.Read:
            this.readPermissionByOperandTypeByClass.set(cls, operandType, id);
            break;
          case Operations.Write:
            this.writePermissionByOperandTypeByClass.set(cls, operandType, id);
            break;
          case Operations.Execute:
            this.executePermissionByOperandTypeByClass.set(
              cls,
              operandType,
              id
            );
            break;
          case Operations.Create:
            throw new Error('Create is not supported');
          default:
            throw new Error('Argument out of range');
        }
      }
    }
  }

  getPermission(
    cls: Class,
    operandType: OperandType,
    operation: Operations
  ): number | undefined {
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
