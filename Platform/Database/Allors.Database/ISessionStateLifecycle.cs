// <copyright file="ISessionState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace Allors
{
    /// <summary>
    /// The Session state's lifecycle.
    /// </summary>
    public interface ISessionStateLifecycle : IDisposable
    {
        void OnInit(ISession session);
    }
}
