// <copyright file="IDatabaseState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using System;

    /// <summary>
    /// The Database state's lifecycle.
    /// </summary>
    public interface IDatabaseStateLifecycle : IDisposable
    {
        void OnInit(IDatabase database);

        ISessionStateLifecycle CreateSessionInstance();
    }
}