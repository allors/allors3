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
        public class CustomerReturnCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdCustomerReturns = changeSet.Created.Select(session.Instantiate).OfType<CustomerReturn>();

                foreach(var customerReturn in createdCustomerReturns)
                {
                    if (!customerReturn.ExistShipToAddress && customerReturn.ExistShipToParty)
                    {
                        customerReturn.ShipToAddress = customerReturn.ShipToParty.ShippingAddress;
                    }

                    if (!customerReturn.ExistShipFromAddress && customerReturn.ExistShipFromParty)
                    {
                        customerReturn.ShipFromAddress = customerReturn.ShipFromParty.ShippingAddress;
                    }

                    Sync(customerReturn);
                }

                void Sync(CustomerReturn customerReturn)
                {
                    // session.Prefetch(this.SyncPrefetch, this);
                    foreach (ShipmentItem shipmentItem in customerReturn.ShipmentItems)
                    {
                        shipmentItem.Sync(customerReturn);
                    }
                }
            }
        }

        public static void CustomerReturnRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("b04d7ed8-fe83-4596-8dd6-32d428fa45f9")] = new CustomerReturnCreationDerivation();
        }
    }
}
