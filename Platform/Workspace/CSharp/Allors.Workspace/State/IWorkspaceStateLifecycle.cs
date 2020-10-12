// <copyright file="IWorkspaceStateLifecycle.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System;

    /// <summary>
    /// The InternalWorkspace stateLifecycle.
    /// </summary>
    public interface IWorkspaceStateLifecycle : IDisposable
    {
        void OnInit(IWorkspace internalWorkspace);

        ISessionStateLifecycle CreateSessionState();
    }
}
