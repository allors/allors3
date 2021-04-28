// <copyright file="RemotePermission.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using Meta;

    internal class Permission
    {
        internal Permission(long id, IClass @class, IOperandType operandType, Operations operation)
        {
            this.Id = id;
            this.Class = @class;
            this.OperandType = operandType;
            this.Operation = operation;
        }

        internal long Id { get; }

        internal IClass Class { get; }

        internal IOperandType OperandType { get; }

        internal Operations Operation { get; }
    }
}
