// <copyright file="IDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    /// <summary>
    /// Scope local to the database.
    /// </summary>
    public interface IDatabaseScope
    {
        void OnInit(IDatabase database);

        ISessionScope CreateSessionScope();
    }
}
