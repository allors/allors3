// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class OrderAdjustmentCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdOrderAdjustment = changeSet.Created.Select(session.Instantiate).OfType<OrderAdjustment>();

                foreach(var orderAdjustment in createdOrderAdjustment)
                {
                    validation.AssertAtLeastOne(orderAdjustment, M.OrderAdjustment.Amount, M.ShippingAndHandlingCharge.Percentage);
                    validation.AssertExistsAtMostOne(orderAdjustment, M.OrderAdjustment.Amount, M.ShippingAndHandlingCharge.Percentage);
                }
            }
        }

        public static void OrderAdjustmentRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("6950ec0a-95c8-490e-8b73-5fcbdfaac4be")] = new OrderAdjustmentCreationDerivation();
        }
    }
}
