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
        public class ShipmentValueCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdShipmentValue = changeSet.Created.Select(v=>v.GetObject()).OfType<ShipmentValue>();

                foreach(var shipmentValue in createdShipmentValue)
                {
                    validation.AssertAtLeastOne(shipmentValue, M.ShipmentValue.FromAmount, M.ShipmentValue.ThroughAmount);
                }
            }
        }

        public static void ShipmentValueRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("4951f65a-39fc-4fa8-9877-050648c552ea")] = new ShipmentValueCreationDerivation();
        }
    }
}
