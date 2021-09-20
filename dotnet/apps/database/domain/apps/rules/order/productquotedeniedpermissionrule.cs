// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class ProductQuoteDeniedPermissionRule : Rule
    {
        public ProductQuoteDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("5629cded-4afb-4ca7-9c78-24c998b8698c")) =>
            this.Patterns = new Pattern[]
        {
            m.ProductQuote.RolePattern(v => v.TransitionalRevocations),
            m.ProductQuote.RolePattern(v => v.ValidQuoteItems),
            m.ProductQuote.RolePattern(v => v.Request),
            m.ProductQuote.AssociationPattern(v => v.SalesOrderWhereQuote, m.ProductQuote),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ProductQuote>())
            {
                @this.Revocations = @this.TransitionalRevocations;

                var deleteRevocation = new Revocations(@this.Strategy.Transaction).ProductQuoteDeleteRevocation;
                var setReadyForProcessingRevocation = new Revocations(@this.Strategy.Transaction).ProductQuoteSetReadyForProcessingRevocation;

                if (@this.QuoteState.IsCreated)
                {
                    if (@this.ExistValidQuoteItems)
                    {
                        @this.RemoveRevocation(setReadyForProcessingRevocation);
                    }
                    else
                    {
                        @this.AddRevocation(setReadyForProcessingRevocation);
                    }
                }

                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Delete);
                if (@this.IsDeletable())
                {
                    @this.RemoveRevocation(deleteRevocation);
                }
                else
                {
                    @this.AddRevocation(deleteRevocation);
                }
            }
        }
    }
}
