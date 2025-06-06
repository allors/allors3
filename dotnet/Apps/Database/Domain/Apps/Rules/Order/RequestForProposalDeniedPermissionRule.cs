// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class RequestForProposalDeniedPermissionRule : Rule
    {
        public RequestForProposalDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("1eb65d1c-7164-4c98-aef5-47c3e96f26d7")) =>
            this.Patterns = new Pattern[]
        {
            m.RequestForProposal.RolePattern(v => v.TransitionalRevocations),
            m.RequestItem.RolePattern(v => v.RequestItemState, v => v.RequestWhereRequestItem, m.RequestForProposal),
            m.Request.AssociationPattern(v => v.QuoteWhereRequest,m.RequestForProposal),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RequestForProposal>())
            {
                @this.Revocations = @this.TransitionalRevocations;

                var revocation = new Revocations(@this.Strategy.Transaction).RequestForProposalDeleteRevocation;
                if (@this.IsDeletable())
                {
                    @this.RemoveRevocation(revocation);
                }
                else
                {
                    @this.AddRevocation(revocation);
                }
            }
        }
    }
}
