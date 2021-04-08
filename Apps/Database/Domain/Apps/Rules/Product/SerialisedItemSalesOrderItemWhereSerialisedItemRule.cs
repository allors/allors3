// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class SerialisedItemSalesOrderItemWhereSerialisedItemRule : Rule
    {
        public SerialisedItemSalesOrderItemWhereSerialisedItemRule(MetaPopulation m) : base(m, new Guid("725888da-ee53-4510-8806-43fd89ec02f2")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.AssociationPattern(v => v.SalesOrderItemsWhereSerialisedItem),
                m.SalesOrderItem.RolePattern(v => v.SalesOrderItemState, v => v.SerialisedItem),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.OnSalesOrder = @this.SalesOrderItemsWhereSerialisedItem.Any(v => v.SalesOrderItemState.IsProvisional
                            || v.SalesOrderItemState.IsReadyForPosting || v.SalesOrderItemState.IsRequestsApproval
                            || v.SalesOrderItemState.IsAwaitingAcceptance || v.SalesOrderItemState.IsOnHold || v.SalesOrderItemState.IsInProcess);
            }
        }
    }
}
