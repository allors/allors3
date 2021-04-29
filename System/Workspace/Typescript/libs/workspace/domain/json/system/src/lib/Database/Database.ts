import { AuthenticationTokenRequest, PullRequest, PullResponse, PullArgs, SecurityRequest, SyncRequest, SyncResponse, PushRequest, InvokeRequest, InvokeResponse, PushResponse, SecurityResponse } from '@allors/protocol/json/system';
import { IObject } from '@allors/workspace/domain/system';
import { Class, MetaPopulation, OperandType } from '@allors/workspace/meta/system';
import { DatabaseObject } from './DatabaseObject';
import { AccessControl } from './Security/AccessControl';
import { Permission } from './Security/Permission';
import { ResponseContext } from './Security/ResponseContext';

export class Database {
  objectsById: Map<number, DatabaseObject>;

  readPermissionByOperandTypeByClass: Map<Class, Map<OperandType, Permission>>;
  writePermissionByOperandTypeByClass: Map<Class, Map<OperandType, Permission>>;
  executePermissionByOperandTypeByClass: Map<Class, Map<OperandType, Permission>>;

  constructor(public metaPopulation: MetaPopulation, public httpClient: HttpClient, public identities: Identities) {
    // this.HttpClient.DefaultRequestHeaders.Accept.Clear();
    // this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    this.accessControlById = new Map();
    this.permissionById = new Map();

    this.objectsById = new Map();

    this.readPermissionByOperandTypeByClass = new Map();
    this.writePermissionByOperandTypeByClass = new Map();
    this.executePermissionByOperandTypeByClass = new Map();
  }

  userId: number;

  accessControlById: Map<number, AccessControl>;

  permissionById: Map<number, Permission>;

  async login(url: string, username: string, password: string): boolean {
    var request: AuthenticationTokenRequest = { l = username, p = password };
    // var response = await this.PostAsJsonAsync(url, request);
    // _ = response.EnsureSuccessStatusCode();
    // var authResult = await this.ReadAsAsync<AuthenticationTokenResponse>(response);
    // if (!authResult.Authenticated)
    // {
    //     return false;
    // }

    // this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);
    // this.UserId = authResult.UserId;

    // return true;
  }

  pushResponse(identity: number, cls: Class): DatabaseObject {
    var databaseObject = new DatabaseObject(this, identity, cls);
    this.objectsById[identity] = databaseObject;
    return databaseObject;
  }

  syncResponse(syncResponse: SyncResponse): SecurityRequest {
    var ctx = new ResponseContext(this.AccessControlById, this.PermissionById);
    for (var syncResponseObject of syncResponse.Objects) {
      var databaseObjects = new DatabaseObject(this, ctx, syncResponseObject);
      this.objectsById.set(databaseObjects.Identity, databaseObjects);
    }

    if (ctx.missingAccessControlIds.length > 0 || ctx.missingPermissionIds.length > 0) {
      return {
        a: Array.from(ctx.missingAccessControlIds),
        p: Array.from(ctx.missingPermissionIds),
      };
    }

    return null;
  }

  diff(response: PullResponse): SyncRequest {
    // return {
    //     Objects = response.p
    //         .filter(v =>
    //         {
    //             if (!this.objectsById.has(v.i))
    //             {
    //                 return true;
    //             }
    //             if (!databaseObject.version.Equals(v.v))
    //             {
    //                 return true;
    //             }
    //             if (v.accessControls === null)
    //             {
    //                 if (databaseObject.accessControlIds.length > 0)
    //                 {
    //                     return true;
    //                 }
    //             }
    //             else if (!databaseObject.accessControlIds.SetEquals(v.AccessControls))
    //             {
    //                 return true;
    //             }
    //             if (v.deniedPermissions === null)
    //             {
    //                 if (databaseObject.deniedPermissionIds.length > 0)
    //                 {
    //                     return true;
    //                 }
    //             }
    //             else
    //             {
    //                 if (databaseObject.deniedPermissionIds.setEquals(v.d))
    //                 {
    //                     return false;
    //                 }
    //                 if (!databaseObject.deniedPermissionIds.isProperSubsetOf(v.d))
    //                 {
    //                     return true;
    //                 }
    //                 databaseObject.updateDeniedPermissions(v.d);
    //             }
    //             return false;
    //         })
    //         .Select(v => v.Id).ToArray(),
    //       }
  }

  get(identity: number): DatabaseObject {
    return this.objectsById.get(identity);
  }

  SecurityResponse(securityResponse: SecurityResponse): SecurityRequest {
    if (securityResponse.p != null) {
      for (const syncResponsePermission of securityResponse.p) {
        // var id = syncResponsePermission[0];
        // var @class = (IClass)this.MetaPopulation.FindByTag((int)syncResponsePermission[1]);
        // var metaObject = this.MetaPopulation.FindByTag((int)syncResponsePermission[2]);
        // var operandType = (IOperandType)(metaObject as IRelationType)?.RoleType ?? (IMethodType)metaObject;
        // var operation = (Operations)syncResponsePermission[3];
        // var permission = new Permission(id, @class, operandType, operation);
        // this.PermissionById[id] = permission;
        // switch (operation)
        // {
        //     case Operations.Read:
        //         if (!this.readPermissionByOperandTypeByClass.TryGetValue(@class,
        //             out var readPermissionByOperandType))
        //         {
        //             readPermissionByOperandType = new Dictionary<IOperandType, Permission>();
        //             this.readPermissionByOperandTypeByClass[@class] = readPermissionByOperandType;
        //         }
        //         readPermissionByOperandType[operandType] = permission;
        //         break;
        //     case Operations.Write:
        //         if (!this.writePermissionByOperandTypeByClass.TryGetValue(@class,
        //             out var writePermissionByOperandType))
        //         {
        //             writePermissionByOperandType = new Dictionary<IOperandType, Permission>();
        //             this.writePermissionByOperandTypeByClass[@class] = writePermissionByOperandType;
        //         }
        //         writePermissionByOperandType[operandType] = permission;
        //         break;
        //     case Operations.Execute:
        //         if (!this.executePermissionByOperandTypeByClass.TryGetValue(@class,
        //             out var executePermissionByOperandType))
        //         {
        //             executePermissionByOperandType = new Dictionary<IOperandType, Permission>();
        //             this.executePermissionByOperandTypeByClass[@class] = executePermissionByOperandType;
        //         }
        //         executePermissionByOperandType[operandType] = permission;
        //         break;
        // }
      }
    }

    let missingPermissionIds: Set<number>;
    if (securityResponse.a != null) {
      for (const syncResponseAccessControl of securityResponse.a) {
        const id = syncResponseAccessControl.i;
        const version = syncResponseAccessControl.v;
        const permissionsIds = syncResponseAccessControl.p?.map((v) => {
          if (this.permissionById.has(v)) {
            return v;
          }

          (missingPermissionIds ??= new Set()).add(v);

          return v;
        });

        var permissionIdSet = permissionsIds != null ? new Set(permissionsIds) : new Set();

        this.accessControlById.set(id, new AccessControl(id, version, permissionIdSet));
      }
    }

    if (!!missingPermissionIds) {
      return {
        p = Array.from(missingPermissionIds),
      };
    }

    return null;
  }

  getPermission(cls: Class, operandType: OperandType, operation: Operations): Permission {
    // switch (operation)
    // {
    //     case Operations.Read:
    //         if (this.readPermissionByOperandTypeByClass.TryGetValue(@class, out var readPermissionByOperandType))
    //         {
    //             if (readPermissionByOperandType.TryGetValue(operandType, out var readPermission))
    //             {
    //                 return readPermission;
    //             }
    //         }
    //         return null;
    //     case Operations.Write:
    //         if (this.writePermissionByOperandTypeByClass.TryGetValue(@class, out var writePermissionByOperandType))
    //         {
    //             if (writePermissionByOperandType.TryGetValue(operandType, out var writePermission))
    //             {
    //                 return writePermission;
    //             }
    //         }
    //         return null;
    //     default:
    //         if (this.executePermissionByOperandTypeByClass.TryGetValue(@class, out var executePermissionByOperandType))
    //         {
    //             if (executePermissionByOperandType.TryGetValue(operandType, out var executePermission))
    //             {
    //                 return executePermission;
    //             }
    //         }
    //         return null;
    // }
  }

  pull(pullRequest: PullRequest): Observable<PullResponse> {
    // var uri = new Uri("pull", UriKind.Relative);
    // var response = await this.PostAsJsonAsync(uri, pullRequest);
    // _ = response.EnsureSuccessStatusCode();
    // return await this.ReadAsAsync<PullResponse>(response);
  }

  pull(name: string, values?: Map<string, object>, objects: Map<string, IObject>, collections: Map<string, IObject[]>): Observable<PullResponse> {
    // const pullArgs: PullArgs = {
    //     v: values?.ToDictionary(v => v.Key, v => v.Value),
    //     o: objects?.ToDictionary(v => v.Key, v => v.Value.Id),
    //     c: collections?.ToDictionary(v => v.Key, v => v.Value.Select(v => v.Id).ToArray()),
    // };
    // var uri = new Uri(name + "/pull", UriKind.Relative);
    // var response = await this.PostAsJsonAsync(uri, pullArgs);
    // _ = response.EnsureSuccessStatusCode();
    // return await this.ReadAsAsync<PullResponse>(response);
  }

  sync(syncRequest: SyncRequest): Observable<SyncResponse> {
    // var uri = new Uri("sync", UriKind.Relative);
    // var response = await this.PostAsJsonAsync(uri, syncRequest);
    // _ = response.EnsureSuccessStatusCode();
    // return await this.ReadAsAsync<SyncResponse>(response);
  }

  push(pushRequest: PushRequest): Observable<PushResponse> {
    // var uri = new Uri("push", UriKind.Relative);
    // var response = await this.PostAsJsonAsync(uri, pushRequest);
    // _ = response.EnsureSuccessStatusCode();
    // return await this.ReadAsAsync<PushResponse>(response);
  }

  invoke(invokeRequest: InvokeRequest): Observable<InvokeResponse> {
    // var uri = new Uri("invoke", UriKind.Relative);
    // var response = await this.PostAsJsonAsync(uri, invokeRequest);
    // _ = response.EnsureSuccessStatusCode();
    // return await this.ReadAsAsync<InvokeResponse>(response);
  }

  security(securityRequest: SecurityRequest): Observable<SecurityResponse> {
    // var uri = new Uri("security", UriKind.Relative);
    // var response = await this.PostAsJsonAsync(uri, securityRequest);
    // _ = response.EnsureSuccessStatusCode();
    // return await this.ReadAsAsync<SecurityResponse>(response);
  }
}
