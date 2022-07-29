// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using Database.Security;
    using Domain;
    using Services;

    public class WorkspaceAclsService : IWorkspaceAclsService
    {
        public ISecurity Security { get; }

        public IWorkspaceMask WorkspaceMask { get; set; }

        public User User { get; }

        public WorkspaceAclsService(ISecurity security, IWorkspaceMask workspaceMask, User user)
        {
            this.Security = security;
            this.WorkspaceMask = workspaceMask;
            this.User = user;
        }

        public IAccessControl Create(string workspaceName)
        {
            var masks = this.WorkspaceMask.GetMasks(workspaceName);
            return new WorkspaceAccessControl(this.Security, this.User, masks, workspaceName);
        }
    }
}
