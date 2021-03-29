// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class QuoteItemDeniedPermissionRule : Rule
    {
        public QuoteItemDeniedPermissionRule(M m) : base(m, new Guid("04ca22ef-d3b0-40f7-9f60-4c4bf5dc10d7")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.QuoteItem, m.QuoteItem.TransitionalDeniedPermissions),
            new RolePattern(m.Quote, m.Quote.TransitionalDeniedPermissions) { Steps = new IPropertyType[] { m.Quote.QuoteItems}},
            new RolePattern(m.Quote, m.Quote.Request) { Steps = new IPropertyType[] { m.Quote.QuoteItems}},
            new AssociationPattern(m.SalesOrder.Quote) { Steps = new IPropertyType[] { m.Quote.QuoteItems}},
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<QuoteItem>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;

                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta.ObjectType, @this.Meta.Delete);
                if (@this.QuoteWhereQuoteItem.IsDeletable())
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
                }
            }
        }
    }
}
