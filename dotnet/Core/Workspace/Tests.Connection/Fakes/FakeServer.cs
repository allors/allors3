// <copyright file="FakeServer.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Connection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Workspace.Meta;

    /// <summary>
    /// A server in memory: objects, grants, revocations and permissions in the shape the
    /// protocol delivers them, answering the six calls as the server's Api does, and keeping
    /// every request it received. A test changes the population between calls to make the
    /// server answer differently.
    /// </summary>
    public sealed class FakeServer
    {
        public FakeServer(M m, IUnitConvert unitConvert)
        {
            this.M = m;
            this.UnitConvert = unitConvert;
        }

        public M M { get; }

        public IUnitConvert UnitConvert { get; }

        public Dictionary<long, FakeObject> Objects { get; } = new Dictionary<long, FakeObject>();

        public Dictionary<long, FakeGrant> Grants { get; } = new Dictionary<long, FakeGrant>();

        public Dictionary<long, FakeRevocation> Revocations { get; } = new Dictionary<long, FakeRevocation>();

        public Dictionary<long, FakePermission> Permissions { get; } = new Dictionary<long, FakePermission>();

        public List<PullRequest> PullRequests { get; } = new List<PullRequest>();

        public List<SyncRequest> SyncRequests { get; } = new List<SyncRequest>();

        public List<AccessRequest> AccessRequests { get; } = new List<AccessRequest>();

        public List<PermissionRequest> PermissionRequests { get; } = new List<PermissionRequest>();

        public List<PushRequest> PushRequests { get; } = new List<PushRequest>();

        public List<InvokeRequest> InvokeRequests { get; } = new List<InvokeRequest>();

        public FakeObject AddObject(long id, IClass @class, params long[] grants)
        {
            var @object = new FakeObject(id, @class) { Grants = grants };
            this.Objects[id] = @object;
            return @object;
        }

        public FakeGrant AddGrant(long id, params long[] permissions)
        {
            var grant = new FakeGrant(id) { Permissions = permissions };
            this.Grants[id] = grant;
            return grant;
        }

        public FakeRevocation AddRevocation(long id, params long[] permissions)
        {
            var revocation = new FakeRevocation(id) { Permissions = permissions };
            this.Revocations[id] = revocation;
            return revocation;
        }

        public FakePermission AddPermission(long id, IClass @class, IOperandType operandType, Operations operation)
        {
            var permission = new FakePermission(id, @class, operandType, operation);
            this.Permissions[id] = permission;
            return permission;
        }

        public PullResponse Pull(PullRequest request)
        {
            this.PullRequests.Add(request);

            var objects = this.Objects.Values.Where(v => Matches(request, v)).OrderBy(v => v.Id).ToArray();

            var versionByGrant = new Dictionary<long, long>();
            var versionByRevocation = new Dictionary<long, long>();
            foreach (var @object in objects)
            {
                foreach (var grantId in @object.Grants)
                {
                    versionByGrant[grantId] = this.Grants[grantId].Version;
                }

                foreach (var revocationId in @object.Revocations)
                {
                    versionByRevocation[revocationId] = this.Revocations[revocationId].Version;
                }
            }

            return new PullResponse
            {
                p = objects.Select(v => new PullResponseObject { i = v.Id, v = v.Version, g = v.Grants.OrderBy(w => w).ToArray(), r = v.Revocations.OrderBy(w => w).ToArray() }).ToArray(),
                c = new Dictionary<string, long[]> { ["Pool"] = objects.Select(v => v.Id).ToArray() },
                o = new Dictionary<string, long>(),
                v = new Dictionary<string, object>(),
                g = versionByGrant.Count > 0 ? versionByGrant.Select(v => new[] { v.Key, v.Value }).ToArray() : null,
                r = versionByRevocation.Count > 0 ? versionByRevocation.Select(v => new[] { v.Key, v.Value }).ToArray() : null,
            };
        }

        public SyncResponse Sync(SyncRequest request)
        {
            this.SyncRequests.Add(request);

            return new SyncResponse
            {
                o = request.o.Where(this.Objects.ContainsKey).Select(id => this.Objects[id]).Select(v => new SyncResponseObject
                {
                    i = v.Id,
                    v = v.Version,
                    c = v.Class.Tag,
                    g = v.Grants.OrderBy(w => w).ToArray(),
                    r = v.Revocations.OrderBy(w => w).ToArray(),
                    ro = v.Roles.Select(kvp => this.ToSyncResponseRole(kvp.Key, kvp.Value)).ToArray(),
                }).ToArray(),
            };
        }

        public AccessResponse Access(AccessRequest request)
        {
            this.AccessRequests.Add(request);

            return new AccessResponse
            {
                g = request.g?.Where(this.Grants.ContainsKey).Select(id => this.Grants[id]).Select(v => new AccessResponseGrant { i = v.Id, v = v.Version, p = v.Permissions.OrderBy(w => w).ToArray() }).ToArray(),
                r = request.r?.Where(this.Revocations.ContainsKey).Select(id => this.Revocations[id]).Select(v => new AccessResponseRevocation { i = v.Id, v = v.Version, p = v.Permissions.OrderBy(w => w).ToArray() }).ToArray(),
            };
        }

        public PermissionResponse Permission(PermissionRequest request)
        {
            this.PermissionRequests.Add(request);

            return new PermissionResponse
            {
                p = request.p?.Where(this.Permissions.ContainsKey).Select(id => this.Permissions[id]).Select(v => new PermissionResponsePermission
                {
                    i = v.Id,
                    c = v.Class.Tag,
                    t = v.OperandType is IRoleType roleType ? roleType.RelationType.Tag : ((IMethodType)v.OperandType).Tag,
                    o = (long)v.Operation,
                }).ToArray(),
            };
        }

        public PushResponse Push(PushRequest request)
        {
            this.PushRequests.Add(request);

            var response = new PushResponse();

            if (request.o != null)
            {
                foreach (var changed in request.o)
                {
                    var @object = this.Objects[changed.d];
                    if (@object.Version != changed.v)
                    {
                        response._v = (response._v ?? Array.Empty<long>()).Concat(new[] { changed.d }).ToArray();
                        continue;
                    }

                    @object.Version++;
                    foreach (var role in changed.r ?? Array.Empty<PushRequestRole>())
                    {
                        var roleType = ((IRelationType)this.M.FindByTag(role.t)).RoleType;
                        if (roleType.ObjectType.IsUnit)
                        {
                            @object.Roles[roleType] = this.UnitConvert.UnitFromJson(roleType.ObjectType.Tag, role.u);
                        }
                        else if (roleType.IsOne)
                        {
                            @object.Roles[roleType] = role.c;
                        }
                        else
                        {
                            var current = (@object.Roles.TryGetValue(roleType, out var value) ? (long[])value : Array.Empty<long>()).ToList();
                            current.AddRange(role.a ?? Array.Empty<long>());
                            current.RemoveAll(v => role.r?.Contains(v) == true);
                            @object.Roles[roleType] = current.OrderBy(v => v).ToArray();
                        }
                    }
                }
            }

            return response;
        }

        public InvokeResponse Invoke(InvokeRequest request)
        {
            this.InvokeRequests.Add(request);
            return new InvokeResponse();
        }

        private static bool Matches(PullRequest request, FakeObject @object)
        {
            if (request.l == null || request.l.Length == 0)
            {
                return true;
            }

            return request.l.Any(pull =>
                pull.o == @object.Id ||
                (pull.e?.t != null && (@object.Class.Tag == pull.e.t || @object.Class.Supertypes.Any(v => v.Tag == pull.e.t))) ||
                (pull.o == null && pull.e == null));
        }

        private SyncResponseRole ToSyncResponseRole(IRoleType roleType, object value)
        {
            var role = new SyncResponseRole { t = roleType.RelationType.Tag };

            if (roleType.ObjectType.IsUnit)
            {
                role.v = this.UnitConvert.ToJson(value);
            }
            else if (roleType.IsOne)
            {
                role.o = (long?)value;
            }
            else
            {
                role.c = ((long[])value).OrderBy(v => v).ToArray();
            }

            return role;
        }
    }

    public sealed class FakeObject
    {
        public FakeObject(long id, IClass @class)
        {
            this.Id = id;
            this.Class = @class;
            this.Version = 1;
        }

        public long Id { get; }

        public IClass Class { get; }

        public long Version { get; set; }

        /// <summary>
        /// A unit value, the id of a composite role as a long, or the ids of a composites role as
        /// a long array.
        /// </summary>
        public Dictionary<IRoleType, object> Roles { get; } = new Dictionary<IRoleType, object>();

        public long[] Grants { get; set; } = Array.Empty<long>();

        public long[] Revocations { get; set; } = Array.Empty<long>();

        public FakeObject WithRole(IRoleType roleType, object value)
        {
            this.Roles[roleType] = value;
            return this;
        }
    }

    public sealed class FakeGrant
    {
        public FakeGrant(long id)
        {
            this.Id = id;
            this.Version = 1;
        }

        public long Id { get; }

        public long Version { get; set; }

        public long[] Permissions { get; set; } = Array.Empty<long>();
    }

    public sealed class FakeRevocation
    {
        public FakeRevocation(long id)
        {
            this.Id = id;
            this.Version = 1;
        }

        public long Id { get; }

        public long Version { get; set; }

        public long[] Permissions { get; set; } = Array.Empty<long>();
    }

    public sealed class FakePermission
    {
        public FakePermission(long id, IClass @class, IOperandType operandType, Operations operation)
        {
            this.Id = id;
            this.Class = @class;
            this.OperandType = operandType;
            this.Operation = operation;
        }

        public long Id { get; }

        public IClass Class { get; }

        public IOperandType OperandType { get; }

        public Operations Operation { get; }
    }
}
