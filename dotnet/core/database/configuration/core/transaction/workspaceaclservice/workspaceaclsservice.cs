// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using Domain;
    using Security;
    using Services;

    public class WorkspaceAclsService : IWorkspaceAclsService
    {
        public IWorkspaceMask WorkspaceMask { get; set; }

        public User User { get; }

        public WorkspaceAclsService(IWorkspaceMask workspaceMask, User user)
        {
            this.WorkspaceMask = workspaceMask;
            this.User = user;
        }

        public IAccessControl Create(string workspace) => new WorkspaceAccessControl(workspace, this.WorkspaceMask, this.User);
    }
}
