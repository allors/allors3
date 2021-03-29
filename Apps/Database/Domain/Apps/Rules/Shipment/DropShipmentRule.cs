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

    public class DropShipmentRule : Rule
    {
        public DropShipmentRule(M m) : base(m, new Guid("1B7E3857-425A-4946-AB63-15AEE196350D")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.DropShipment, m.DropShipment.ShipFromParty),
                new RolePattern(m.DropShipment, m.DropShipment.ShipFromAddress),
                new RolePattern(m.DropShipment, m.DropShipment.ShipToParty),
                new RolePattern(m.DropShipment, m.DropShipment.ShipToAddress),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<DropShipment>())
            {
                if (!@this.ExistShipmentNumber && @this.ExistStore)
                {
                    var year = @this.Transaction().Now().Year;
                    @this.ShipmentNumber = @this.Store.NextDropShipmentNumber(year);

                    var fiscalYearStoreSequenceNumbers = @this.Store.FiscalYearsStoreSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = ((InternalOrganisation)@this.ShipFromParty).DropShipmentSequence.IsEnforcedSequence ? @this.Store.DropShipmentNumberPrefix : fiscalYearStoreSequenceNumbers.DropShipmentNumberPrefix;
                    @this.SortableShipmentNumber = @this.Transaction().GetSingleton().SortableNumber(prefix, @this.ShipmentNumber, year.ToString());
                }

                if (!@this.ExistShipToAddress && @this.ExistShipToParty)
                {
                    @this.ShipToAddress = @this.ShipToParty.ShippingAddress;
                }

                if (!@this.ExistShipFromAddress && @this.ExistShipFromParty)
                {
                    @this.ShipFromAddress = @this.ShipFromParty.ShippingAddress;
                }
            }
        }
    }
}
