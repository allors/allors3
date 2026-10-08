// <copyright file="Permission.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using Meta;

    /// <summary>
    /// A permission as the server defines it: an operation on an operand type of a class.
    /// </summary>
    public sealed class Permission
    {
        public Permission(long id, IClass @class, IOperandType operandType, Operations operation)
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
