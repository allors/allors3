// <copyright file="Permission.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Database.Security;
    using Meta;

    public partial interface Permission : IPermission
    {
        bool ExistClass { get; }

        bool ExistOperandType { get; }

        bool ExistOperation { get; }

        IOperandType OperandType { get; }

        Operations Operation { get; }
    }
}
