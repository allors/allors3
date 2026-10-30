// <copyright file="RemoteDatabaseObject.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api.Sync;
    using Meta;
    using Ranges;

    internal sealed class DatabaseRecord : Adapters.DatabaseRecord
    {
        private readonly DatabaseConnection database;
        private readonly Dictionary<IRelationType, object> roles;
        private readonly ResponseContext.AccessEntry[] grants;
        private readonly ResponseContext.AccessEntry[] revocations;

        private DatabaseRecord(DatabaseConnection database, IClass @class, long id, long version, string binding,
            Dictionary<IRelationType, object> roles, IEnumerable<ResponseContext.AccessEntry> grants,
            IEnumerable<ResponseContext.AccessEntry> revocations) : base(@class, id, version)
        {
            this.database = database;
            this.Key = (id, version, binding);
            this.roles = roles;
            this.grants = grants.ToArray();
            this.revocations = revocations.ToArray();
        }

        internal (long Id, long Version, string Security) Key { get; }

        internal static DatabaseRecord FromResponse(DatabaseConnection database, SyncResponseObject response, string binding,
            IEnumerable<ResponseContext.AccessEntry> grants, IEnumerable<ResponseContext.AccessEntry> revocations)
        {
            var meta = database.Configuration.MetaPopulation;
            var @class = meta.FindByTag(response.c) as IClass;
            if (@class == null || response.i <= 0 || response.v < Allors.Version.DatabaseInitial.Value)
            {
                throw ResponseContext.Invalid("Sync contains an invalid object identity or class");
            }

            var roles = new Dictionary<IRelationType, object>();
            foreach (var value in response.ro ?? throw ResponseContext.Invalid("Sync must include each object role table"))
            {
                var relation = value == null ? null : meta.FindByTag(value.t) as IRelationType;
                var role = relation?.RoleType;
                if (role == null)
                {
                    throw ResponseContext.Invalid($"Sync object {response.i} (class '{response.c}') references unknown role tag '{value?.t ?? "<null>"}'. Check that the client and server use matching workspace models; regenerate your workspace if needed.");
                }

                if (!@class.DatabaseOriginRoleTypes.Contains(role))
                {
                    throw ResponseContext.Invalid($"Sync object {response.i} (class '{response.c}') contains role tag '{value.t}' that is not a database role of this class.");
                }

                if (roles.ContainsKey(relation))
                {
                    throw ResponseContext.Invalid($"Sync object {response.i} (class '{response.c}') contains duplicate role tag '{value.t}'. Return each role only once.");
                }

                object decoded;
                if (role.ObjectType.IsUnit)
                {
                    if (value.o != null || value.c != null)
                    {
                        throw ResponseContext.Invalid("Sync contains an invalid unit role");
                    }

                    if (!database.ValidUnitType(role.ObjectType.Tag, value.v))
                    {
                        throw ResponseContext.Invalid("Sync contains a value with the wrong JSON unit type");
                    }

                    decoded = database.UnitConvert.UnitFromJson(role.ObjectType.Tag, value.v);
                    if (!ValidUnit(role.ObjectType.Tag, decoded))
                    {
                        throw ResponseContext.Invalid("Sync contains a value with the wrong unit type");
                    }

                    if (decoded is byte[] bytes)
                    {
                        decoded = bytes.ToArray();
                    }
                }
                else if (role.IsOne)
                {
                    if (value.v != null || value.c != null || value.o < 0)
                    {
                        throw ResponseContext.Invalid("Sync contains an invalid to-one role");
                    }

                    decoded = value.o == 0 ? null : value.o;
                }
                else
                {
                    if (value.v != null || value.o != null)
                    {
                        throw ResponseContext.Invalid("Sync contains an invalid to-many role");
                    }

                    decoded = database.Ranges.Load(ResponseContext.Ids(value.c));
                }

                roles.Add(relation, decoded);
            }

            return new DatabaseRecord(database, @class, response.i, response.v, binding, roles, grants, revocations);
        }

        public override object GetRole(IRoleType roleType)
        {
            this.roles.TryGetValue(roleType.RelationType, out var value);
            if (value is byte[] bytes)
            {
                return bytes.ToArray();
            }

            if (value is IRange<long> range)
            {
                return this.database.Ranges.Load(range.ToArray());
            }

            return value;
        }

        public override bool IsPermitted(long permission) =>
            this.grants.Any(v => v.Contains(permission)) && !this.revocations.Any(v => v.Contains(permission));

        internal bool SameContents(DatabaseRecord other) =>
            this.Class == other.Class && this.roles.Count == other.roles.Count && this.roles.All(entry =>
                other.roles.TryGetValue(entry.Key, out var value) && Equal(entry.Value, value));

        private static bool Equal(object left, object right)
        {
            if (left is byte[] bytes)
            {
                return right is byte[] otherBytes && bytes.SequenceEqual(otherBytes);
            }

            if (left is IRange<long> range)
            {
                return right is IRange<long> otherRange && range.SequenceEqual(otherRange);
            }

            return Equals(left, right);
        }

        private static bool ValidUnit(string tag, object value) => value == null || tag switch
        {
            UnitTags.Binary => value is byte[],
            UnitTags.Boolean => value is bool,
            UnitTags.DateTime => value is DateTime,
            UnitTags.Decimal => value is decimal,
            UnitTags.Float => value is double number && !double.IsNaN(number) && !double.IsInfinity(number),
            UnitTags.Integer => value is int,
            UnitTags.String => value is string,
            UnitTags.Unique => value is Guid,
            _ => false,
        };
    }
}
