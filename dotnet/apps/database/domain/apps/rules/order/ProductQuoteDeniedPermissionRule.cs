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

    public class ProductQuoteDeniedPermissionRule : Rule
    {
        public ProductQuoteDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("5629cded-4afb-4ca7-9c78-24c998b8698c")) =>
            this.Patterns = new Pattern[]
        {
            m.ProductQuote.RolePattern(v => v.TransitionalDeniedPermissions),
            m.ProductQuote.RolePattern(v => v.ValidQuoteItems),
            m.ProductQuote.RolePattern(v => v.Request),
            m.ProductQuote.AssociationPattern(v => v.SalesOrderWhereQuote, m.ProductQuote),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ProductQuote>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;

                var SetReadyPermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.SetReadyForProcessing);

                if (@this.QuoteState.IsCreated)
                {
                    if (@this.ExistValidQuoteItems)
                    {
                        @this.RemoveDeniedPermission(SetReadyPermission);
                    }
                    else
                    {
                        @this.AddDeniedPermission(SetReadyPermission);
                    }
                }

                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Delete);
                if (@this.IsDeletable())
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
