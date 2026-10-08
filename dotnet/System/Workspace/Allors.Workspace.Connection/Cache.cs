// <copyright file="Cache.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using System;
    using System.Collections.Generic;
    using Meta;

    /// <summary>
    /// The private cache of a connection: plain dictionaries, for one thread at a time.
    /// </summary>
    internal sealed class Cache : ICache
    {
        private readonly Dictionary<long, IRecord> recordById = new Dictionary<long, IRecord>();
        private readonly Dictionary<long, Grant> grantById = new Dictionary<long, Grant>();
        private readonly Dictionary<long, Revocation> revocationById = new Dictionary<long, Revocation>();
        private readonly HashSet<long> permissionIds = new HashSet<long>();

        private readonly Dictionary<IClass, Dictionary<IOperandType, long>> readPermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, long>>();
        private readonly Dictionary<IClass, Dictionary<IOperandType, long>> writePermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, long>>();
        private readonly Dictionary<IClass, Dictionary<IOperandType, long>> executePermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, long>>();

        public IRecord GetRecord(long id)
        {
            this.recordById.TryGetValue(id, out var record);
            return record;
        }

        public void SetRecord(IRecord record) => this.recordById[record.Id] = record;

        public Grant GetGrant(long id)
        {
            this.grantById.TryGetValue(id, out var grant);
            return grant;
        }

        public void SetGrant(Grant grant) => this.grantById[grant.Id] = grant;

        public Revocation GetRevocation(long id)
        {
            this.revocationById.TryGetValue(id, out var revocation);
            return revocation;
        }

        public void SetRevocation(Revocation revocation) => this.revocationById[revocation.Id] = revocation;

        public bool HasPermission(long id) => this.permissionIds.Contains(id);

        public void SetPermission(Permission permission)
        {
            var permissionByOperandTypeByClass = this.PermissionByOperandTypeByClass(permission.Operation);

            this.permissionIds.Add(permission.Id);

            if (!permissionByOperandTypeByClass.TryGetValue(permission.Class, out var permissionByOperandType))
            {
                permissionByOperandType = new Dictionary<IOperandType, long>();
                permissionByOperandTypeByClass[permission.Class] = permissionByOperandType;
            }

            permissionByOperandType[permission.OperandType] = permission.Id;
        }

        public long GetPermission(IClass @class, IOperandType operandType, Operations operation) =>
            this.PermissionByOperandTypeByClass(operation).TryGetValue(@class, out var permissionByOperandType) && permissionByOperandType.TryGetValue(operandType, out var permission)
                ? permission
                : 0;

        private Dictionary<IClass, Dictionary<IOperandType, long>> PermissionByOperandTypeByClass(Operations operation) =>
            operation switch
            {
                Operations.Read => this.readPermissionByOperandTypeByClass,
                Operations.Write => this.writePermissionByOperandTypeByClass,
                Operations.Execute => this.executePermissionByOperandTypeByClass,
                Operations.Create => throw new NotSupportedException("Create is not supported"),
                _ => throw new ArgumentOutOfRangeException(nameof(operation)),
            };
    }
}
