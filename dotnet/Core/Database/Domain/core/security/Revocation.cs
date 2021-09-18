// <copyright file="AccessControl.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;
    using Database.Security;

    public partial class Revocation : IRevocation
    {
        IPermission[] IRevocation.Permissions => this.DeniedPermissions.ToArray();

        // TODO: Optimize
        public bool InWorkspace(string workspaceName) => this.DeniedPermissions.Any(v => v.OperandType.WorkspaceNames.Contains(workspaceName));
    }
}
