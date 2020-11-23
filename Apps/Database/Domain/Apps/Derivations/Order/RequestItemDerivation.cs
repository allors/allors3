// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class RequestItemDerivation : DomainDerivation
    {
        public RequestItemDerivation(M m) : base(m, new Guid("764C2996-50E5-4C53-A6DA-A527BCECF221")) =>
            this.Patterns = new[]
            {
                new ChangedPattern(this.M.RequestItem.RequestItemState)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RequestItem>())
            {
                validation.AssertAtLeastOne(@this, this.M.RequestItem.Product, this.M.RequestItem.ProductFeature, this.M.RequestItem.SerialisedItem, this.M.RequestItem.Description, this.M.RequestItem.NeededSkill, this.M.RequestItem.Deliverable);
                validation.AssertExistsAtMostOne(@this, this.M.RequestItem.Product, this.M.RequestItem.ProductFeature, this.M.RequestItem.Description, this.M.RequestItem.NeededSkill, this.M.RequestItem.Deliverable);
                validation.AssertExistsAtMostOne(@this, this.M.RequestItem.SerialisedItem, this.M.RequestItem.ProductFeature, this.M.RequestItem.Description, this.M.RequestItem.NeededSkill, this.M.RequestItem.Deliverable);

                var requestItemStates = new RequestItemStates(cycle.Session);
                if (@this.IsValid)
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
                    @this.UnitOfMeasure = new UnitsOfMeasure(@this.Strategy.Session).Piece;
                }

                if (@this.ExistRequestWhereRequestItem && new RequestStates(@this.Strategy.Session).Cancelled.Equals(@this.RequestWhereRequestItem.RequestState))
                {
                    @this.Cancel();
                }

                if (@this.ExistSerialisedItem && @this.Quantity != 1)
                {
                    validation.AddError($"{@this} {@this.Meta.Quantity} {ErrorMessages.SerializedItemQuantity}");
                }
            }
        }
    }
}
