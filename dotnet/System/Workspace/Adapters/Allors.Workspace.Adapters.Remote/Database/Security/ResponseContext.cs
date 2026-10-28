// <copyright file="RemoteResponseContext.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;
    using Meta;

    internal sealed class ResponseContext
    {
        private readonly DatabaseConnection database;
        private readonly Dictionary<(long Id, long Version), AccessEntry> knownGrants;
        private readonly Dictionary<(long Id, long Version), AccessEntry> knownRevocations;
        private readonly HashSet<long> requested = new HashSet<long>();

        internal ResponseContext(DatabaseConnection database, PullResponse response,
            Dictionary<(long Id, long Version, string Security), DatabaseRecord> records,
            Dictionary<(long Id, long Version), AccessEntry> grants,
            Dictionary<(long Id, long Version), AccessEntry> revocations)
        {
            this.database = database;
            var grantVersions = Versions(response.g);
            var revocationVersions = Versions(response.r);
            this.knownGrants = grantVersions.Select(v => (v.Key, v.Value)).Where(grants.ContainsKey).ToDictionary(v => v, v => grants[v]);
            this.knownRevocations = revocationVersions.Select(v => (v.Key, v.Value)).Where(revocations.ContainsKey).ToDictionary(v => v, v => revocations[v]);
            var ids = new HashSet<long>();
            foreach (var item in response.p ?? throw Invalid("Pull pool is missing"))
            {
                if (item == null || item.i <= 0 || item.v < Allors.Version.DatabaseInitial.Value || !ids.Add(item.i)) throw Invalid("Pull contains an invalid or duplicate object");
                var key = (item.i, item.v, Binding(item.g, item.r, grantVersions, revocationVersions));
                if (records.TryGetValue(key, out var record)) this.Records.Add(item.i, record);
                else this.requested.Add(item.i);
            }

            ValidateResults(response.o, ids);
            if (response.c != null)
            {
                var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var entry in response.c)
                {
                    if (entry.Key == null || !names.Add(entry.Key) || entry.Value == null || entry.Value.Any(id => !ids.Contains(id))) throw Invalid("Pull contains an invalid collection result");
                }
            }

            if (response.v != null && response.v.Keys.Distinct(StringComparer.OrdinalIgnoreCase).Count() != response.v.Count) throw Invalid("Pull contains duplicate value names");
        }

        internal Dictionary<long, DatabaseRecord> Records { get; } = new Dictionary<long, DatabaseRecord>();
        internal List<DatabaseRecord> SynchronizedRecords { get; } = new List<DatabaseRecord>();
        internal Dictionary<(long Id, long Version), AccessEntry> Grants { get; } = new Dictionary<(long, long), AccessEntry>();
        internal Dictionary<(long Id, long Version), AccessEntry> Revocations { get; } = new Dictionary<(long, long), AccessEntry>();
        internal Dictionary<long, PermissionEntry> Permissions { get; } = new Dictionary<long, PermissionEntry>();

        internal SyncRequest SyncRequest => new SyncRequest
        {
            o = this.requested.OrderBy(v => v).ToArray(),
            g = this.knownGrants.Keys.Select(v => new[] { v.Id, v.Version }).ToArray(),
            r = this.knownRevocations.Keys.Select(v => new[] { v.Id, v.Version }).ToArray(),
        };

        internal void OnSync(SyncResponse response)
        {
            if (response?.o == null || response.g == null || response.r == null || response.p == null) throw Invalid("Sync must include object, grant, revocation and permission tables");
            var definitions = new Dictionary<(IClass, IOperandType, Operations), long>();
            foreach (var item in response.p)
            {
                var entry = new PermissionEntry(this.database, item);
                if (this.Permissions.ContainsKey(entry.Id) || definitions.ContainsKey(entry.Key)) throw Invalid("Sync contains duplicate permission definitions");
                this.Permissions.Add(entry.Id, entry);
                definitions.Add(entry.Key, entry.Id);
            }

            var grantVersions = new Dictionary<long, long>();
            foreach (var entry in response.g)
            {
                if (entry == null) throw Invalid("Sync contains a null grant");
                this.AddAccess("grant", entry.i, entry.v, entry.p, this.knownGrants, this.Grants, grantVersions);
            }

            var revocationVersions = new Dictionary<long, long>();
            foreach (var entry in response.r)
            {
                if (entry == null) throw Invalid("Sync contains a null revocation");
                this.AddAccess("revocation", entry.i, entry.v, entry.p, this.knownRevocations, this.Revocations, revocationVersions);
            }

            var received = new HashSet<long>();
            foreach (var item in response.o)
            {
                if (item == null || !this.requested.Contains(item.i) || !received.Add(item.i)) throw Invalid("Sync contains an unrequested or duplicate object");
                var binding = Binding(item.g, item.r, grantVersions, revocationVersions);
                var grants = Ids(item.g).Select(id => this.Grants[(id, grantVersions[id])]);
                var revocations = Ids(item.r).Select(id => this.Revocations[(id, revocationVersions[id])]);
                var record = DatabaseRecord.FromResponse(this.database, item, binding, grants, revocations);
                this.Records.Add(record.Id, record);
                this.SynchronizedRecords.Add(record);
            }

            if (!received.SetEquals(this.requested)) throw Invalid("Sync did not return every requested object");
        }

        internal void ValidatePublication(
            Dictionary<(long Id, long Version, string Security), DatabaseRecord> records,
            Dictionary<(long Id, long Version), AccessEntry> grants,
            Dictionary<(long Id, long Version), AccessEntry> revocations,
            Dictionary<long, PermissionEntry> permissions,
            Dictionary<(IClass Class, IOperandType Operand, Operations Operation), long> permissionIds)
        {
            foreach (var entry in this.Grants) if (grants.TryGetValue(entry.Key, out var known) && !known.SameContents(entry.Value)) throw Invalid($"Sync changed the contents of grant {entry.Key.Id} without changing version {entry.Key.Version}.");
            foreach (var entry in this.Revocations) if (revocations.TryGetValue(entry.Key, out var known) && !known.SameContents(entry.Value)) throw Invalid($"Sync changed the contents of revocation {entry.Key.Id} without changing version {entry.Key.Version}.");
            foreach (var entry in this.Permissions)
            {
                if (permissions.TryGetValue(entry.Key, out var known) && !known.Key.Equals(entry.Value.Key)) throw Invalid($"Sync changed the definition of permission {entry.Key}.");
                if (permissionIds.TryGetValue(entry.Value.Key, out var id) && id != entry.Key) throw Invalid($"Sync permission {entry.Key} conflicts with cached permission {id} for class '{entry.Value.Key.Class.Tag}', operand '{entry.Value.Key.Operand.OperandTag}' and operation {(long)entry.Value.Key.Operation}.");
            }

            foreach (var record in this.Records.Values)
            {
                if (records.TryGetValue(record.Key, out var known) && !known.SameContents(record)) throw Invalid($"Sync changed the contents of object {record.Id} at data version {record.Version} without changing its security versions.");
            }
        }

        internal static InvalidOperationException Invalid(string message) => new InvalidOperationException(message);

        internal static long[] Ids(long[] values)
        {
            var ids = values?.ToArray() ?? Array.Empty<long>();
            if (ids.Any(id => id <= 0) || ids.Distinct().Count() != ids.Length) throw Invalid("An object or security reference contains invalid or duplicate IDs");
            Array.Sort(ids);
            return ids;
        }

        private static Dictionary<long, long> Versions(long[][] entries)
        {
            var versions = new Dictionary<long, long>();
            foreach (var entry in entries ?? Array.Empty<long[]>())
            {
                if (entry == null || entry.Length != 2 || entry[0] <= 0 || entry[1] < 0 || versions.ContainsKey(entry[0])) throw Invalid("Pull contains an invalid or duplicate security version");
                versions.Add(entry[0], entry[1]);
            }

            return versions;
        }

        private static string Binding(long[] grants, long[] revocations, Dictionary<long, long> grantVersions, Dictionary<long, long> revocationVersions)
        {
            string Key(long[] ids, Dictionary<long, long> versions) => string.Join(",", Ids(ids).Select(id =>
            {
                if (!versions.TryGetValue(id, out var version)) throw Invalid("An object references a missing security version");
                return id.ToString(CultureInfo.InvariantCulture) + ":" + version.ToString(CultureInfo.InvariantCulture);
            }));
            return Key(grants, grantVersions) + "|" + Key(revocations, revocationVersions);
        }

        private static void ValidateResults(IDictionary<string, long> objects, HashSet<long> ids)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in objects ?? new Dictionary<string, long>())
            {
                if (entry.Key == null || !names.Add(entry.Key) || (entry.Value != 0 && !ids.Contains(entry.Value))) throw Invalid("Pull contains an invalid object result");
            }
        }

        private void AddAccess(string kind, long id, long version, long[] contents,
            Dictionary<(long Id, long Version), AccessEntry> known,
            Dictionary<(long Id, long Version), AccessEntry> staged, Dictionary<long, long> versions)
        {
            if (id <= 0 || version < 0 || versions.ContainsKey(id)) throw Invalid("Sync contains an invalid or duplicate security identity");
            versions.Add(id, version);
            AccessEntry entry;
            if (contents == null)
            {
                if (!known.TryGetValue((id, version), out entry)) throw Invalid("Sync omitted contents for a security version that was not advertised");
            }
            else
            {
                var ids = Ids(contents);
                if (ids.Any(permission => !this.Permissions.ContainsKey(permission))) throw Invalid("Sync omitted a referenced permission definition");
                entry = new AccessEntry(ids);
                if (known.TryGetValue((id, version), out var previous) && !previous.SameContents(entry)) throw Invalid($"Sync changed the contents of {kind} {id} without changing version {version}.");
            }

            staged.Add((id, version), entry);
        }

        internal sealed class AccessEntry
        {
            private readonly HashSet<long> permissions;
            internal AccessEntry(IEnumerable<long> permissions) => this.permissions = new HashSet<long>(permissions);
            internal bool Contains(long id) => this.permissions.Contains(id);
            internal bool SameContents(AccessEntry other) => this.permissions.SetEquals(other.permissions);
        }

        internal sealed class PermissionEntry
        {
            internal PermissionEntry(DatabaseConnection database, PermissionResponsePermission entry)
            {
                if (entry == null || entry.i <= 0) throw Invalid("Sync contains an invalid permission");
                var meta = database.Configuration.MetaPopulation;
                var @class = meta.FindByTag(entry.c) as IClass;
                var operand = meta.FindByTag(entry.t);
                var operation = (Operations)entry.o;
                IOperandType type = (operand as IRelationType)?.RoleType;
                if (operation == Operations.Execute) type = operand as IMethodType;
                else if (operation != Operations.Read && operation != Operations.Write) throw Invalid("Sync contains an unsupported permission operation");
                if (@class == null || type == null) throw Invalid("Sync contains an unknown permission class or operand");
                if (operation == Operations.Execute ? !@class.MethodTypes.Contains((IMethodType)type) : !@class.DatabaseOriginRoleTypes.Contains((IRoleType)type)) throw Invalid("Sync contains a permission operand that does not belong to its class");
                this.Id = entry.i;
                this.Key = (@class, type, operation);
            }

            internal long Id { get; }
            internal (IClass Class, IOperandType Operand, Operations Operation) Key { get; }
        }
    }
}
