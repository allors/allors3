// <copyright file="RemoteDatabase.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;
    using Meta;
    using Ranges;

    public abstract class DatabaseConnection : Adapters.DatabaseConnection
    {
        private readonly object cacheLock = new object();
        private readonly Dictionary<(long Id, long Version, string Security), DatabaseRecord> records = new Dictionary<(long, long, string), DatabaseRecord>();
        private readonly Dictionary<(long Id, long Version), ResponseContext.AccessEntry> grants = new Dictionary<(long, long), ResponseContext.AccessEntry>();
        private readonly Dictionary<(long Id, long Version), ResponseContext.AccessEntry> revocations = new Dictionary<(long, long), ResponseContext.AccessEntry>();
        private readonly Dictionary<long, ResponseContext.PermissionEntry> permissions = new Dictionary<long, ResponseContext.PermissionEntry>();
        private readonly Dictionary<(IClass Class, IOperandType Operand, Operations Operation), long> permissionIds = new Dictionary<(IClass, IOperandType, Operations), long>();
        private readonly Func<IWorkspaceServices> servicesBuilder;

        protected DatabaseConnection(Adapters.Configuration configuration, IdGenerator idGenerator, Func<IWorkspaceServices> servicesBuilder, IRanges<long> ranges) : base(configuration, idGenerator)
        {
            this.Ranges = ranges;
            this.servicesBuilder = servicesBuilder;
        }

        public abstract IUnitConvert UnitConvert { get; }

        internal IRanges<long> Ranges { get; }

        protected internal virtual bool ValidUnitType(string tag, object value)
        {
            if (value == null)
            {
                return true;
            }

            if (value is string text)
            {
                switch (tag)
                {
                    case UnitTags.Binary:
                        return System.Text.RegularExpressions.Regex.IsMatch(text, "^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$");
                    case UnitTags.Unique:
                        return Guid.TryParseExact(text, "D", out _);
                    case UnitTags.Decimal:
                        return ValidDecimal(text);
                    case UnitTags.DateTime:
                        return System.Text.RegularExpressions.Regex.IsMatch(text, "^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}(?:\\.[0-9]{1,7})?(?:Z|[+-][0-9]{2}:[0-9]{2})?$")
                            && DateTime.TryParseExact(text, new[] { "yyyy-MM-dd'T'HH:mm:ssK", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK" }, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _);
                    case UnitTags.String:
                        return true;
                    default:
                        return false;
                }
            }

            if (value is bool)
            {
                return tag == UnitTags.Boolean;
            }

            if (value is DateTime)
            {
                return tag == UnitTags.DateTime;
            }

            return (value is int || value is long || value is float || value is double || value is decimal)
                && (tag == UnitTags.Integer || tag == UnitTags.Float);
        }

        private static bool ValidDecimal(string text)
        {
            text = text.Trim();
            if (!System.Text.RegularExpressions.Regex.IsMatch(text, "^[+-]?(?:[0-9]+(?:\\.[0-9]*)?|\\.[0-9]+)$"))
            {
                return false;
            }

            var parts = text.TrimStart('+', '-').Split('.');
            var fraction = parts.Length == 2 ? parts[1].TrimEnd('0') : string.Empty;
            var coefficient = (parts[0] + fraction).TrimStart('0');
            const string maximum = "79228162514264337593543950335";
            return fraction.Length <= 28 && (coefficient.Length < maximum.Length
                || coefficient.Length == maximum.Length && string.CompareOrdinal(coefficient, maximum) <= 0);
        }

        public override IWorkspace CreateWorkspace() => new Workspace(this, this.servicesBuilder(), this.Ranges);

        internal ResponseContext OnPullResponse(PullRequest request, PullResponse response)
        {
            lock (this.cacheLock)
            {
                return new ResponseContext(this, request, response, this.records, this.grants, this.revocations);
            }
        }

        internal void Publish(ResponseContext context)
        {
            lock (this.cacheLock)
            {
                // Other sessions may have completed while this response was in flight.
                // Check every identity before publishing any part of this response.
                context.ValidatePublication(this.records, this.grants, this.revocations, this.permissions, this.permissionIds);
                foreach (var entry in context.Grants)
                {
                    this.grants[entry.Key] = entry.Value;
                }

                foreach (var entry in context.Revocations)
                {
                    this.revocations[entry.Key] = entry.Value;
                }

                foreach (var entry in context.Permissions)
                {
                    this.permissions[entry.Key] = entry.Value;
                    this.permissionIds[entry.Value.Key] = entry.Key;
                }

                foreach (var record in context.SynchronizedRecords)
                {
                    this.records[record.Key] = record;
                }
            }
        }

        public override long GetPermission(IClass @class, IOperandType operandType, Operations operation)
        {
            if (operation == Operations.Create)
            {
                throw new NotSupportedException("Create is not supported");
            }

            if (operation != Operations.Read && operation != Operations.Write && operation != Operations.Execute)
            {
                throw new ArgumentOutOfRangeException(nameof(operation));
            }

            lock (this.cacheLock)
            {
                return this.permissionIds.TryGetValue((@class, operandType, operation), out var id) ? id : 0;
            }
        }

        public long[] CachedVersions(long id)
        {
            lock (this.cacheLock)
            {
                var versions = new List<long>();
                foreach (var key in this.records.Keys)
                {
                    if (key.Id == id)
                    {
                        versions.Add(key.Version);
                    }
                }

                versions.Sort();
                return versions.ToArray();
            }
        }

        public abstract Task<PullResponse> Pull(PullRequest pullRequest);
        public abstract Task<SyncResponse> Sync(SyncRequest syncRequest);
        public abstract Task<PushResponse> Push(PushRequest pushRequest);
        public abstract Task<InvokeResponse> Invoke(InvokeRequest invokeRequest);
        public abstract Task<AccessResponse> Access(AccessRequest accessRequest);
        public abstract Task<PermissionResponse> Permission(PermissionRequest permissionRequest);
    }
}
