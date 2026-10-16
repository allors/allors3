// <copyright file="ReadStoredPermission.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;

    internal sealed class ReadStoredPermission
    {
        public ReadStoredPermission(ReadStoreScope scope, long id, string classTag, Operations operation, string operandTag = null)
        {
            this.Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            this.Id = ReadStoreValues.Id(id, nameof(id));
            this.ClassTag = ReadStoreValues.Required(classTag, nameof(classTag));
            if (operation != Operations.Create && operation != Operations.Read && operation != Operations.Write && operation != Operations.Execute)
            {
                throw new ArgumentException("Choose one permission operation: Create, Read, Write, or Execute.", nameof(operation));
            }

            if (operation == Operations.Create ? operandTag != null : string.IsNullOrEmpty(operandTag))
            {
                throw new ArgumentException("Create requires no operand; Read/Write require a relation tag and Execute requires a method tag.", nameof(operandTag));
            }

            this.Operation = operation;
            this.OperandTag = operandTag;
        }

        public ReadStoreScope Scope { get; }

        public long Id { get; }

        public string ClassTag { get; }

        public Operations Operation { get; }

        public string OperandTag { get; }

        internal bool SameBody(ReadStoredPermission other) => this.ClassTag == other.ClassTag && this.Operation == other.Operation && this.OperandTag == other.OperandTag;
    }
}
