// <copyright file="RemoteDatabase.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
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
        private readonly Dictionary<long, DatabaseRecord> recordsById = new Dictionary<long, DatabaseRecord>();
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

        public override IWorkspace CreateWorkspace() => new Workspace(this, this.servicesBuilder(), this.Ranges);

        internal ResponseContext OnPullResponse(PullResponse response)
        {
            lock (this.cacheLock)
            {
                return new ResponseContext(this, response, this.records, this.grants, this.revocations);
            }
        }

        internal void Publish(ResponseContext context)
        {
            lock (this.cacheLock)
            {
                // Other sessions may have completed while this response was in flight.
                // Check every identity before publishing any part of this response.
                foreach (var record in context.Records.Values)
                {
                    if (this.recordsById.TryGetValue(record.Id, out var previous) && previous.Class != record.Class) throw ResponseContext.Invalid("An object's class changed");
                }

                context.ValidatePublication(this.records, this.grants, this.revocations, this.permissions, this.permissionIds);
                foreach (var entry in context.Grants) this.grants[entry.Key] = entry.Value;
                foreach (var entry in context.Revocations) this.revocations[entry.Key] = entry.Value;
                foreach (var entry in context.Permissions)
                {
                    this.permissions[entry.Key] = entry.Value;
                    this.permissionIds[entry.Value.Key] = entry.Key;
                }

                foreach (var record in context.SynchronizedRecords)
                {
                    this.records[record.Key] = record;
                    this.recordsById[record.Id] = record;
                }
            }
        }

        public override long GetPermission(IClass @class, IOperandType operandType, Operations operation)
        {
            if (operation != Operations.Read && operation != Operations.Write && operation != Operations.Execute)
            {
                throw new NotSupportedException($"Operation {operation} is not supported");
            }

            lock (this.cacheLock)
            {
                return this.permissionIds.TryGetValue((@class, operandType, operation), out var id) ? id : 0;
            }
        }

        public override Adapters.DatabaseRecord GetRecord(long id)
        {
            lock (this.cacheLock)
            {
                return this.recordsById.TryGetValue(id, out var record) ? record : null;
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
