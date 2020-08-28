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

    public static partial class DabaseExtensions
    {
        public class RequestItemCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdRequestItem = changeSet.Created.Select(session.Instantiate).OfType<RequestItem>();

                foreach (var requestItem in createdRequestItem)
                {

                    validation.AssertAtLeastOne(requestItem, M.RequestItem.Product, M.RequestItem.ProductFeature, M.RequestItem.SerialisedItem, M.RequestItem.Description, M.RequestItem.NeededSkill, M.RequestItem.Deliverable);
                    validation.AssertExistsAtMostOne(requestItem, M.RequestItem.Product, M.RequestItem.ProductFeature, M.RequestItem.Description, M.RequestItem.NeededSkill, M.RequestItem.Deliverable);
                    validation.AssertExistsAtMostOne(requestItem, M.RequestItem.SerialisedItem, M.RequestItem.ProductFeature, M.RequestItem.Description, M.RequestItem.NeededSkill, M.RequestItem.Deliverable);

                    var requestItemStates = new RequestItemStates(session);
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

                    var deletePermission = new Permissions(requestItem.Strategy.Session).Get(requestItem.Meta.ObjectType, requestItem.Meta.Delete, Operations.Execute);
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
        public static void RequestItemRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("d6f70dc1-1e91-458e-ab53-7cef431fbd26")] = new RequestItemCreationDerivation();
        }
    }
}
