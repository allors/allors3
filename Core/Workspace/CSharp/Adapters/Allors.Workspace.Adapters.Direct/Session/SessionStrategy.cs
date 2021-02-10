// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Direct
{
    using System;
    using Meta;

    public class SessionStrategy : Strategy
    {
        public SessionStrategy(Session session, IClass @class, long workspaceId) : base(session, workspaceId, @class)
        {
        }

        internal override State GetPopulation(Origin origin) =>
            origin switch
            {
                Origin.Workspace => this.Session.Workspace.State,
                Origin.Session => this.Session.State,
                _ => throw new Exception($"Unsupported origin: {origin}")
            };
    }
}
