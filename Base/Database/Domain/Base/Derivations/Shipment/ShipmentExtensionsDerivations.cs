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
        public class ShipmentExtensionsCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdShipment = changeSet.Created.Select(session.Instantiate).OfType<Shipment>();

                foreach (var shipment in createdShipment)
                {
                    shipment.AddSecurityToken(new SecurityTokens(session).DefaultSecurityToken);
                }
            }
        }

        public static void ShipmentExtensionsRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("f23469b2-eed5-4261-b12f-44c9114afd1a")] = new ShipmentExtensionsCreationDerivation();
        }
    }
}
