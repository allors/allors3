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
    using Derivations.Rules;

    public class RequestItemRule : Rule
    {
        public RequestItemRule(MetaPopulation m) : base(m, new Guid("764C2996-50E5-4C53-A6DA-A527BCECF221")) =>
            this.Patterns = new Pattern[]
            {
                m.RequestItem.RolePattern(v => v.RequestItemState),
                m.RequestItem.RolePattern(v => v.UnitOfMeasure),
                m.Request.RolePattern(v => v.RequestState, v => v.RequestItems),
                m.RequestItem.AssociationPattern(v => v.RequestWhereRequestItem),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<RequestItem>())
            {
                var requestItemStates = new RequestItemStates(cycle.Transaction);
                if (@this.ExistRequestWhereRequestItem && @this.IsValid)
                {
                    if (@this.RequestWhereRequestItem.RequestState.IsSubmitted && @this.RequestItemState.IsDraft)
                    {
                        @this.RequestItemState = requestItemStates.Submitted;
                    }

                    if (@this.RequestWhereRequestItem.RequestState.IsCancelled)
                    {
                        @this.RequestItemState = requestItemStates.Cancelled;
                    }

                    if (@this.RequestWhereRequestItem.RequestState.IsRejected)
                    {
                        @this.RequestItemState = requestItemStates.Rejected;
                    }

                    if (@this.RequestWhereRequestItem.RequestState.IsQuoted)
                    {
                        @this.RequestItemState = requestItemStates.Quoted;
                    }
                }

                if (!@this.ExistUnitOfMeasure)
                {
                    @this.UnitOfMeasure = new UnitsOfMeasure(@this.Strategy.Transaction).Piece;
                }
            }
        }
    }
}
