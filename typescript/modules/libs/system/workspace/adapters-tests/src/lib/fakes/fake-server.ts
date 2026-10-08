import {
  AccessRequest,
  AccessResponse,
  InvokeRequest,
  InvokeResponse,
  PermissionRequest,
  PermissionResponse,
  PullRequest,
  PullResponse,
  PushRequest,
  PushResponse,
  Response,
  SyncRequest,
  SyncResponse,
  SyncResponseRole,
} from '@allors/system/common/protocol-json';
import {
  metaPopulationFingerprint,
  Operations,
} from '@allors/system/workspace/connection';
import { RelationType } from '@allors/system/workspace/meta';
import {
  Class,
  MethodType,
  OperandType,
  RoleType,
} from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';

const ascending = (a: number, b: number) => a - b;

/**
 * A server in memory: objects, grants, revocations and permissions in the shape the
 * protocol delivers them, answering the six calls as the server's Api does, and keeping
 * every request it received. A test changes the population between calls to make the
 * server answer differently.
 */
export class FakeServer {
  /**
   * The database the fake server says it is; null makes it say nothing.
   */
  databaseId: string | null = 'fake';

  /**
   * The user the fake server says it serves the requests as.
   */
  userId: number | null = 1;

  /**
   * The workspace name the fake server says it serves.
   */
  workspaceName = 'Default';

  /**
   * The meta fingerprint the fake server says it has; that of its meta population by default.
   */
  metaFingerprint: string | null;

  readonly objects = new Map<number, FakeObject>();

  readonly grants = new Map<number, FakeGrant>();

  readonly revocations = new Map<number, FakeRevocation>();

  readonly permissions = new Map<number, FakePermission>();

  readonly pullRequests: PullRequest[] = [];

  readonly syncRequests: SyncRequest[] = [];

  readonly accessRequests: AccessRequest[] = [];

  readonly permissionRequests: PermissionRequest[] = [];

  readonly pushRequests: PushRequest[] = [];

  readonly invokeRequests: InvokeRequest[] = [];

  constructor(public readonly m: M) {
    this.metaFingerprint = metaPopulationFingerprint(m);
  }

  addObject(id: number, cls: Class, ...grants: number[]): FakeObject {
    const object = new FakeObject(id, cls);
    object.grants = grants;
    this.objects.set(id, object);
    return object;
  }

  addGrant(id: number, ...permissions: number[]): FakeGrant {
    const grant = new FakeGrant(id);
    grant.permissions = permissions;
    this.grants.set(id, grant);
    return grant;
  }

  addRevocation(id: number, ...permissions: number[]): FakeRevocation {
    const revocation = new FakeRevocation(id);
    revocation.permissions = permissions;
    this.revocations.set(id, revocation);
    return revocation;
  }

  addPermission(
    id: number,
    cls: Class,
    operandType: OperandType,
    operation: Operations
  ): FakePermission {
    const permission = new FakePermission(id, cls, operandType, operation);
    this.permissions.set(id, permission);
    return permission;
  }

  pull(request: PullRequest): PullResponse {
    this.pullRequests.push(request);

    const objects = [...this.objects.values()]
      .filter((v) => this.matches(request, v))
      .sort((a, b) => a.id - b.id);

    const versionByGrant = new Map<number, number>();
    const versionByRevocation = new Map<number, number>();
    for (const object of objects) {
      for (const grantId of object.grants) {
        versionByGrant.set(grantId, this.grants.get(grantId).version);
      }

      for (const revocationId of object.revocations) {
        versionByRevocation.set(
          revocationId,
          this.revocations.get(revocationId).version
        );
      }
    }

    return this.envelope<PullResponse>({
      p: objects.map((v) => ({
        i: v.id,
        v: v.version,
        g: [...v.grants].sort(ascending),
        r: [...v.revocations].sort(ascending),
      })),
      c: { Pool: objects.map((v) => v.id) },
      o: {},
      v: {},
      g:
        versionByGrant.size > 0
          ? [...versionByGrant].map(([id, version]) => [id, version])
          : null,
      r:
        versionByRevocation.size > 0
          ? [...versionByRevocation].map(([id, version]) => [id, version])
          : null,
    });
  }

  sync(request: SyncRequest): SyncResponse {
    this.syncRequests.push(request);

    return this.envelope<SyncResponse>({
      o: request.o
        .filter((id) => this.objects.has(id))
        .map((id) => this.objects.get(id))
        .map((v) => ({
          i: v.id,
          v: v.version,
          c: v.cls.tag,
          g: [...v.grants].sort(ascending),
          r: [...v.revocations].sort(ascending),
          ro: [...v.roles].map(([roleType, value]) =>
            this.toSyncResponseRole(roleType, value)
          ),
        })),
    });
  }

  access(request: AccessRequest): AccessResponse {
    this.accessRequests.push(request);

    return this.envelope<AccessResponse>({
      g: request.g
        ?.filter((id) => this.grants.has(id))
        .map((id) => this.grants.get(id))
        .map((v) => ({
          i: v.id,
          v: v.version,
          p: [...v.permissions].sort(ascending),
        })),
      r: request.r
        ?.filter((id) => this.revocations.has(id))
        .map((id) => this.revocations.get(id))
        .map((v) => ({
          i: v.id,
          v: v.version,
          p: [...v.permissions].sort(ascending),
        })),
    });
  }

  permission(request: PermissionRequest): PermissionResponse {
    this.permissionRequests.push(request);

    return this.envelope<PermissionResponse>({
      p: request.p
        ?.filter((id) => this.permissions.has(id))
        .map((id) => this.permissions.get(id))
        .map((v) => ({
          i: v.id,
          c: v.cls.tag,
          t:
            (v.operandType as RoleType).relationType?.tag ??
            (v.operandType as MethodType).tag,
          o: v.operation,
        })),
    });
  }

  push(request: PushRequest): PushResponse {
    this.pushRequests.push(request);

    const response = this.envelope<PushResponse>({ n: null });

    if (request.o != null) {
      for (const changed of request.o) {
        const object = this.objects.get(changed.d);
        if (object.version !== changed.v) {
          response._v = [...(response._v ?? []), changed.d];
          continue;
        }

        object.version++;
        for (const role of changed.r ?? []) {
          const roleType = (
            this.m.metaObjectByTag.get(role.t) as RelationType
          ).roleType;
          if (roleType.objectType.isUnit) {
            object.roles.set(roleType, role.u);
          } else if (roleType.isOne) {
            object.roles.set(roleType, role.c);
          } else {
            const current = [
              ...((object.roles.get(roleType) as number[]) ?? []),
              ...(role.a ?? []),
            ].filter((v) => !(role.r?.includes(v) ?? false));
            object.roles.set(roleType, current.sort(ascending));
          }
        }
      }
    }

    return response;
  }

  invoke(request: InvokeRequest): InvokeResponse {
    this.invokeRequests.push(request);
    return this.envelope<InvokeResponse>({});
  }

  private envelope<T extends Response>(response: Partial<T>): T {
    const enveloped = response as T;
    enveloped._db = this.databaseId;
    enveloped._u = this.userId;
    enveloped._w = this.workspaceName;
    enveloped._f = this.metaFingerprint;
    return enveloped;
  }

  private matches(request: PullRequest, object: FakeObject): boolean {
    if (request.l == null || request.l.length === 0) {
      return true;
    }

    return request.l.some(
      (pull) =>
        pull.o === object.id ||
        (pull.e?.t != null &&
          (object.cls.tag === pull.e.t ||
            [...object.cls.supertypes].some((v) => v.tag === pull.e.t))) ||
        (pull.o == null && pull.e == null)
    );
  }

  private toSyncResponseRole(roleType: RoleType, value: unknown): SyncResponseRole {
    const role: SyncResponseRole = { t: roleType.relationType.tag } as SyncResponseRole;

    if (roleType.objectType.isUnit) {
      role.v = value as string;
    } else if (roleType.isOne) {
      role.o = value as number;
    } else {
      role.c = [...(value as number[])].sort(ascending);
    }

    return role;
  }
}

export class FakeObject {
  version = 1;

  /**
   * A unit value, the id of a composite role, or the ids of a composites role.
   */
  readonly roles = new Map<RoleType, unknown>();

  grants: number[] = [];

  revocations: number[] = [];

  constructor(public readonly id: number, public readonly cls: Class) {}

  withRole(roleType: RoleType, value: unknown): FakeObject {
    this.roles.set(roleType, value);
    return this;
  }
}

export class FakeGrant {
  version = 1;

  permissions: number[] = [];

  constructor(public readonly id: number) {}
}

export class FakeRevocation {
  version = 1;

  permissions: number[] = [];

  constructor(public readonly id: number) {}
}

export class FakePermission {
  constructor(
    public readonly id: number,
    public readonly cls: Class,
    public readonly operandType: OperandType,
    public readonly operation: Operations
  ) {}
}

