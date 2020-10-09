// <copyright file="IWorkspace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using Protocol.Database.Pull;
    using Protocol.Database.Sync;
    using Domain;
    using Meta;
    using Protocol.Database.Security;

    public interface IWorkspace
    {
        IWorkspaceLifecycle Lifecycle { get; }

        IObjectFactory ObjectFactory { get; }

        ISession CreateSession();

        SyncRequest Diff(PullResponse response);

        SecurityRequest Sync(SyncResponse syncResponse);

        SecurityRequest Security(SecurityResponse securityResponse);

        IWorkspaceObject Get(long id);

        Permission GetPermission(IClass @class, IOperandType roleType, Operations operation);
    }
}
