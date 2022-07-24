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

    public class RequestForQuoteDeniedPermissionRule : Rule
    {
        public RequestForQuoteDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("eb67ef60-1a60-4b52-85ac-979fb9346242")) =>
            this.Patterns = new Pattern[]
        {
            m.RequestForQuote.RolePattern(v => v.TransitionalRevocations),
            m.RequestItem.RolePattern(v => v.RequestItemState, v => v.RequestWhereRequestItem, m.RequestForQuote),
            m.Request.AssociationPattern(v => v.QuoteWhereRequest, m.RequestForQuote),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RequestForQuote>())
            {
                @this.Revocations = @this.TransitionalRevocations;

                var deleteRevocation = new Revocations(@this.Strategy.Transaction).RequestForQuoteDeleteRevocation;
                var submitRevocation = new Revocations(@this.Strategy.Transaction).RequestForQuoteSubmitRevocation;

                if (@this.ExistOriginator)
                {
                    @this.RemoveRevocation(submitRevocation);
                }
                else
                {
                    @this.AddRevocation(submitRevocation);
                }

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
