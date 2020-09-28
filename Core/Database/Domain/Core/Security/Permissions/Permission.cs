// <copyright file="Permission.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using Allors.Meta;

    public partial interface Permission
    {
        ObjectType ConcreteClass { get; set; }

        bool ExistConcreteClass { get; }

        bool ExistOperandType { get; }

        bool ExistOperation { get; }

        OperandType OperandType { get; }

        Operations Operation { get; }
    }
}