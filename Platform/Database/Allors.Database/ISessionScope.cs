// <copyright file="ISessionScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    /// <summary>
    /// Scope local to the session.
    /// </summary>
    public interface ISessionScope
    {
        void OnInit(ISession session);
    }
}
