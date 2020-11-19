// <copyright file="Session.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Direct.Api
{
    using System;

    public class Permission
    {
        public Permission(long id, Guid @class, Guid operandType, Operations operation)
        {
            this.Id = id;
            this.Class = @class;
            this.OperandType = operandType;
            this.Operation = operation;
        }

        public long Id { get; }

        public Guid Class { get; }

        public Guid OperandType { get; }

        public Operations Operation { get; }
    }
}
