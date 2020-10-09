// <copyright file="ISessionScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace Allors.Workspace
{
    /// <summary>
    /// The Context lifecycle.
    /// </summary>
    public interface ISessionLifecycle : IDisposable
    {
        void OnInit(IContext context);
    }
}
