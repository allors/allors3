// <copyright file="ChangeSet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the AllorsChangeSetMemory type.
// </summary>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    internal sealed class ChangeLog
    {
        private readonly HashSet<Strategy> created;
        private readonly HashSet<IStrategy> deleted;


        internal ChangeLog()
        {
            this.created = new HashSet<Strategy>();
            this.deleted = new HashSet<IStrategy>();
        }

        internal void OnCreated(Strategy strategy) => this.created.Add(strategy);

        internal void OnDeleted(Strategy strategy) => this.deleted.Add(strategy);

        internal void OnChangingUnitRole(Strategy association, IRoleType roleType, object role, object previousRole)
        {
        }

        internal ChangeSet Checkpoint() =>
            new ChangeSet(
                this.created != null ? new HashSet<IObject>(this.created.Select(v => v.GetObject())) : null,
                this.deleted,
                null,
                null
            );
    }
}
