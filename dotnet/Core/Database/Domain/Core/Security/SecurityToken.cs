// <copyright file="AccessControl.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;
    using Database.Security;

    public partial class SecurityToken : ISecurityToken
    {
        IGrant[] ISecurityToken.Grants => this.Grants.ToArray();
    }
}
