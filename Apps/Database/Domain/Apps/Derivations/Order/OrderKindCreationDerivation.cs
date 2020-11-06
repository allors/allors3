// <copyright file="PartyFinancialRelationshipCreationDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class OrderKindCreationDerivation : DomainDerivation
    {
        public OrderKindCreationDerivation(M m) : base(m, new Guid("f3efe9a1-d566-4af2-b33c-7ec7cefae552")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(m.OrderKind.Class)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<OrderKind>())
            {
                if (!@this.ExistScheduleManually)
                {
                    @this.ScheduleManually = false;
                }
            }
        }
    }
}
