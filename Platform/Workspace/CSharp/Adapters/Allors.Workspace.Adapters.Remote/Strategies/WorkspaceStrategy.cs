// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using Allors.Workspace.Meta;

    public class WorkspaceStrategy : NonDatabaseStrategy
    {
        public WorkspaceStrategy(Session session, IClass @class, long workspaceId) : base(session, @class, workspaceId)
        {
        }

        internal override Population GetPopulation(Origin origin) =>
            origin switch
            {
                Origin.Workspace => this.Session.Workspace.Population,
                _ => throw new Exception($"Unsupported origin: {origin}")
            };
    }
}
