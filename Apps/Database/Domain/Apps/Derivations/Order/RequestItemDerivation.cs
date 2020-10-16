// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public class RequestItemDerivation : DomainDerivation
    {
        public RequestItemDerivation(M m) : base(m, new Guid("764C2996-50E5-4C53-A6DA-A527BCECF221")) =>
            this.Patterns = new[]
            {
                new CreatedPattern(this.M.RequestItem.Class)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var requestItem in matches.Cast<RequestItem>())
            {
                validation.AssertAtLeastOne(requestItem, this.M.RequestItem.Product, this.M.RequestItem.ProductFeature, this.M.RequestItem.SerialisedItem, this.M.RequestItem.Description, this.M.RequestItem.NeededSkill, this.M.RequestItem.Deliverable);
                validation.AssertExistsAtMostOne(requestItem, this.M.RequestItem.Product, this.M.RequestItem.ProductFeature, this.M.RequestItem.Description, this.M.RequestItem.NeededSkill, this.M.RequestItem.Deliverable);
                validation.AssertExistsAtMostOne(requestItem, this.M.RequestItem.SerialisedItem, this.M.RequestItem.ProductFeature, this.M.RequestItem.Description, this.M.RequestItem.NeededSkill, this.M.RequestItem.Deliverable);

                var requestItemStates = new RequestItemStates(cycle.Session);
                if (requestItem.IsValid)
                {
                    if (requestItem.RequestWhereRequestItem.RequestState.IsSubmitted && requestItem.RequestItemState.IsDraft)
                    {
                        requestItem.RequestItemState = requestItemStates.Submitted;
                    }

                    if (requestItem.RequestWhereRequestItem.RequestState.IsCancelled)
                    {
                        requestItem.RequestItemState = requestItemStates.Cancelled;
                    }

                    if (requestItem.RequestWhereRequestItem.RequestState.IsRejected)
                    {
                        requestItem.RequestItemState = requestItemStates.Rejected;
                    }

                    if (requestItem.RequestWhereRequestItem.RequestState.IsQuoted)
                    {
                        requestItem.RequestItemState = requestItemStates.Quoted;
                    }
                }

                if (!requestItem.ExistUnitOfMeasure)
                {
                    requestItem.UnitOfMeasure = new UnitsOfMeasure(requestItem.Strategy.Session).Piece;
                }

                if (requestItem.ExistRequestWhereRequestItem && new RequestStates(requestItem.Strategy.Session).Cancelled.Equals(requestItem.RequestWhereRequestItem.RequestState))
                {
                    requestItem.Cancel();
                }

                if (requestItem.ExistSerialisedItem && requestItem.Quantity != 1)
                {
                    validation.AddError($"{requestItem} {requestItem.Meta.Quantity} {ErrorMessages.SerializedItemQuantity}");
                }

                var deletePermission = new Permissions(requestItem.Strategy.Session).Get(requestItem.Meta.ObjectType, requestItem.Meta.Delete);
                if (requestItem.IsDeletable)
                {
                    requestItem.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    requestItem.AddDeniedPermission(deletePermission);
                }
            }
        }
    }
}
