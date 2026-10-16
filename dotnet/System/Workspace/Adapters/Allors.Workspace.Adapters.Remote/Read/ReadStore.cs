// <copyright file="ReadStore.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Connection-owned immutable read cache. Operations are synchronized;
    /// enumeration and defensive input copying occur before the lock. Publication accepts immutable
    /// values and validates their complete closure before changing any current index.
    /// </summary>
    internal sealed class ReadStore
    {
        private readonly object gate = new object();
        private readonly Dictionary<string, ReadStoredRecord> records = new Dictionary<string, ReadStoredRecord>(StringComparer.Ordinal);
        private readonly Dictionary<ReadSecurityKey, ReadStoredSecurity> security = new Dictionary<ReadSecurityKey, ReadStoredSecurity>();
        private readonly Dictionary<long, ReadStoredPermission> permissions = new Dictionary<long, ReadStoredPermission>();
        private readonly Dictionary<long, ReadStoredRecord> currentRecords = new Dictionary<long, ReadStoredRecord>();
        private readonly Dictionary<string, ReadStoredSecurity> currentSecurity = new Dictionary<string, ReadStoredSecurity>(StringComparer.Ordinal);
        private readonly Dictionary<long, ReadStoredPermission> currentPermissions = new Dictionary<long, ReadStoredPermission>();
        private readonly HashSet<long> unavailable = new HashSet<long>();

        public ReadStore(ReadStoreScope scope) => this.Scope = scope ?? throw new ArgumentNullException(nameof(scope));

        public ReadStoreScope Scope { get; }

        /// <summary>
        /// Finds an exact candidate by immutable scope/header identity. This is not a latest-by-object lookup.
        /// </summary>
        public ReadStoredRecord GetRecord(ReadStoredRecord candidate)
        {
            if (candidate == null)
            {
                throw new ArgumentNullException(nameof(candidate));
            }

            lock (this.gate)
            {
                this.RequireScope(candidate.Scope);
                return this.records.TryGetValue(candidate.Identity, out var result) ? result : null;
            }
        }

        public ReadStoredRecord GetRecord(long id, string classTag, long dataVersion,
            IEnumerable<ReadSecurityKey> grants, IEnumerable<ReadSecurityKey> revocations) =>
            this.GetRecord(new ReadStoredRecord(this.Scope, id, classTag, dataVersion, grants, revocations, new Dictionary<string, object>()));

        public ReadStoredSecurity GetSecurity(ReadSecurityKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock (this.gate)
            {
                return this.security.TryGetValue(key, out var result) ? result : null;
            }
        }

        public ReadStoredPermission GetPermission(long id)
        {
            ReadStoreValues.Id(id, nameof(id));
            lock (this.gate)
            {
                return this.permissions.TryGetValue(id, out var result) ? result : null;
            }
        }

        public ReadStoredRecord GetCurrentRecord(long id)
        {
            ReadStoreValues.Id(id, nameof(id));
            lock (this.gate)
            {
                return this.currentRecords.TryGetValue(id, out var result) ? result : null;
            }
        }

        public ReadStoredSecurity GetCurrentSecurity(string kind, long id)
        {
            var key = new ReadSecurityKey(kind, id, 0).CurrentKey;
            lock (this.gate)
            {
                return this.currentSecurity.TryGetValue(key, out var result) ? result : null;
            }
        }

        public ReadStoredPermission GetCurrentPermission(long id)
        {
            ReadStoreValues.Id(id, nameof(id));
            lock (this.gate)
            {
                return this.currentPermissions.TryGetValue(id, out var result) ? result : null;
            }
        }

        public bool IsUnavailable(long id)
        {
            ReadStoreValues.Id(id, nameof(id));
            lock (this.gate)
            {
                return this.unavailable.Contains(id);
            }
        }

        /// <summary>Evaluates membership, current bodies and descriptors under one publication lock.</summary>
        public bool IsPermitted(long id, string classTag, Operations operation, string operandTag)
        {
            ReadStoreValues.Id(id, nameof(id));
            lock (this.gate)
            {
                if (this.unavailable.Contains(id) || !this.currentRecords.TryGetValue(id, out var membership) || membership.ClassTag != classTag)
                {
                    return false;
                }

                var allowed = false;
                foreach (var key in membership.Revocations.Concat(membership.Grants))
                {
                    if (!this.currentSecurity.TryGetValue(key.CurrentKey, out var body)) { return false; }
                    foreach (var permissionId in body.PermissionIds)
                    {
                        if (!this.currentPermissions.TryGetValue(permissionId, out var permission)) { return false; }
                        if (permission.ClassTag == classTag && permission.Operation == operation && permission.OperandTag == operandTag)
                        {
                            if (key.Kind == "revocation") { return false; }
                            allowed = true;
                        }
                    }
                }

                return allowed;
            }
        }

        /// <summary>
        /// Captures an assembled closure, validates every body and existing identity conflict, then imports
        /// and publishes together. A rejected completion cannot leave newly retained candidate bodies.
        /// </summary>
        public ReadSnapshot ImportAndPublish(IEnumerable<ReadStoredRecord> records, IEnumerable<ReadStoredSecurity> security,
            IEnumerable<ReadStoredPermission> permissions, IEnumerable<long> unavailable = null, System.Threading.CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var recordArray = (records ?? throw new ArgumentNullException(nameof(records)))
                .ToArray();
            var securityArray = (security ?? throw new ArgumentNullException(nameof(security)))
                .ToArray();
            var permissionArray = (permissions ?? throw new ArgumentNullException(nameof(permissions)))
                .ToArray();
            var unavailableArray = (unavailable ?? Array.Empty<long>()).Select(v => ReadStoreValues.Id(v, nameof(unavailable))).Distinct().ToArray();
            foreach (var scope in recordArray.Select(v => v.Scope).Concat(securityArray.Select(v => v.Scope)).Concat(permissionArray.Select(v => v.Scope)))
            {
                this.RequireScope(scope);
            }

            ValidatePublication(recordArray, securityArray, permissionArray, unavailableArray);

            lock (this.gate)
            {
                foreach (var record in recordArray)
                {
                    if (this.records.TryGetValue(record.Identity, out var existing) && !existing.SameBody(record))
                    { throw new InvalidOperationException("The exact scoped record already has a different body. Obtain a consistent completion before publishing."); }
                }

                foreach (var body in securityArray)
                {
                    if (this.security.TryGetValue(body.Key, out var existing) && !existing.SameBody(body))
                    { throw new InvalidOperationException("The exact scoped security key already has a different body. Obtain a consistent completion before publishing."); }
                }

                foreach (var permission in permissionArray)
                {
                    if (this.permissions.TryGetValue(permission.Id, out var existing) && !existing.SameBody(permission))
                    { throw new InvalidOperationException("The scoped permission already has a different descriptor. Use a new representation before publishing changed definitions."); }
                }

                cancellationToken.ThrowIfCancellationRequested();
                // Every validation and external callback is complete. These owned arrays and dictionaries
                // form one synchronized import/publication; cancellation applies before this commit point.
                for (var i = 0; i < recordArray.Length; ++i)
                {
                    var record = recordArray[i];
                    if (this.records.TryGetValue(record.Identity, out var existing)) { recordArray[i] = existing; }
                    else { this.records.Add(record.Identity, record); }
                }

                for (var i = 0; i < securityArray.Length; ++i)
                {
                    var body = securityArray[i];
                    if (this.security.TryGetValue(body.Key, out var existing)) { securityArray[i] = existing; }
                    else { this.security.Add(body.Key, body); }
                }

                for (var i = 0; i < permissionArray.Length; ++i)
                {
                    var permission = permissionArray[i];
                    if (this.permissions.TryGetValue(permission.Id, out var existing)) { permissionArray[i] = existing; }
                    else { this.permissions.Add(permission.Id, permission); }
                }

                return this.PublishValidated(recordArray, securityArray, permissionArray, unavailableArray);
            }
        }

        internal void Clear()
        {
            lock (this.gate)
            {
                this.records.Clear();
                this.security.Clear();
                this.permissions.Clear();
                this.currentRecords.Clear();
                this.currentSecurity.Clear();
                this.currentPermissions.Clear();
                this.unavailable.Clear();
            }
        }

        private static void ValidatePublication(ReadStoredRecord[] recordArray, ReadStoredSecurity[] securityArray,
            ReadStoredPermission[] permissionArray, long[] unavailable)
        {
            if (recordArray.Select(v => v.Id).Distinct().Count() != recordArray.Length ||
                securityArray.Select(v => v.Key.CurrentKey).Distinct().Count() != securityArray.Length ||
                permissionArray.Select(v => v.Id).Distinct().Count() != permissionArray.Length)
            {
                // Keep conflicting immutable bodies distinct from a repeated publication entry.
                if (recordArray.GroupBy(v => v.Identity).Any(group => group.Any(v => !group.First().SameBody(v))))
                {
                    throw new InvalidOperationException("The exact scoped record identity already has a different payload. Obtain a consistent fresh record before retaining it.");
                }

                if (securityArray.GroupBy(v => v.Key).Any(group => group.Any(v => !group.First().SameBody(v))))
                {
                    throw new InvalidOperationException("The exact scoped security key already has different permission IDs. Obtain a consistent versioned body before retaining it.");
                }

                if (permissionArray.GroupBy(v => v.Id).Any(group => group.Any(v => !group.First().SameBody(v))))
                {
                    throw new InvalidOperationException("The scoped permission ID already has a different descriptor. Change the representation scope before retaining a changed definition.");
                }

                throw new ArgumentException("Publish at most one record per object, one version per security kind/ID, and one descriptor per permission ID.");
            }

            if (recordArray.Any(v => unavailable.Contains(v.Id)))
            {
                throw new ArgumentException("An object cannot be both available and unavailable in one publication.", nameof(unavailable));
            }

            var requiredSecurity = new HashSet<ReadSecurityKey>(recordArray.SelectMany(v => v.Grants.Concat(v.Revocations)));
            if (!requiredSecurity.SetEquals(securityArray.Select(v => v.Key)))
            {
                throw new InvalidOperationException("Publish exactly the security bodies required by the records' complete provenance; supply every missing body and remove unrelated bodies.");
            }

            var requiredPermissions = new HashSet<long>(securityArray.SelectMany(v => v.PermissionIds));
            if (!requiredPermissions.SetEquals(permissionArray.Select(v => v.Id)))
            {
                throw new InvalidOperationException("Publish exactly the permission descriptors referenced by the supplied security bodies.");
            }
        }

        private ReadSnapshot PublishValidated(ReadStoredRecord[] recordArray, ReadStoredSecurity[] securityArray,
            ReadStoredPermission[] permissionArray, long[] unavailableArray)
        {
            // Called under the store lock after all validation. Current security determines which
            // permission descriptors stay retained, independently of historical record provenance.
            foreach (var record in recordArray)
            {
                this.currentRecords[record.Id] = record;
                this.unavailable.Remove(record.Id);
            }

            foreach (var body in securityArray)
            {
                this.currentSecurity[body.Key.CurrentKey] = body;
            }

            foreach (var descriptor in permissionArray)
            {
                this.currentPermissions[descriptor.Id] = descriptor;
            }

            var currentPermissionIds = new HashSet<long>(this.currentSecurity.Values.SelectMany(v => v.PermissionIds));
            foreach (var id in this.currentPermissions.Keys.Where(v => !currentPermissionIds.Contains(v)).ToArray())
            {
                this.currentPermissions.Remove(id);
            }

            foreach (var id in unavailableArray)
            {
                this.currentRecords.Remove(id);
                this.unavailable.Add(id);
            }

            return new ReadSnapshot(this.Scope, recordArray, securityArray, permissionArray);
        }

        private void RequireScope(ReadStoreScope scope)
        {
            if (!this.Scope.Equals(scope))
            {
                throw new InvalidOperationException("The read contents belong to a different scope. Use a new store for that population, user, workspace, representation, or context.");
            }
        }

    }

    internal sealed class ReadSnapshot
    {
        internal ReadSnapshot(ReadStoreScope scope, ReadStoredRecord[] records, ReadStoredSecurity[] security, ReadStoredPermission[] permissions)
        {
            this.Scope = scope;
            this.Records = Array.AsReadOnly(records);
            this.Security = Array.AsReadOnly(security);
            this.Permissions = Array.AsReadOnly(permissions);
        }

        internal ReadStoreScope Scope { get; }
        internal IReadOnlyList<ReadStoredRecord> Records { get; }
        internal IReadOnlyList<ReadStoredSecurity> Security { get; }
        internal IReadOnlyList<ReadStoredPermission> Permissions { get; }
    }
}
