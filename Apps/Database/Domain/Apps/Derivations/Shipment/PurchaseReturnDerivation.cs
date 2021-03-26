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
    using Database.Derivations;

    public class PurchaseReturnDerivation : DomainDerivation
    {
        public PurchaseReturnDerivation(M m) : base(m, new Guid("B5AB3B14-310A-42EE-9EF5-963290D812CC")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PurchaseReturn, m.PurchaseReturn.Store),
                new RolePattern(m.PurchaseReturn, m.PurchaseReturn.ShipFromParty),
                new RolePattern(m.PurchaseReturn, m.PurchaseReturn.ShipFromAddress),
                new RolePattern(m.PurchaseReturn, m.PurchaseReturn.ShipToParty),
                new RolePattern(m.PurchaseReturn, m.PurchaseReturn.ShipToAddress),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseReturn>())
            {
                if (!@this.ExistShipmentNumber && @this.ExistStore)
                {
                    var year = @this.Transaction().Now().Year;
                    @this.ShipmentNumber = @this.Store.NextPurchaseReturnNumber(year);

                    var fiscalYearStoreSequenceNumbers = @this.Store.FiscalYearsStoreSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = ((InternalOrganisation)@this.ShipFromParty).PurchaseReturnSequence.IsEnforcedSequence ? @this.Store.PurchaseReturnNumberPrefix : fiscalYearStoreSequenceNumbers.PurchaseReturnNumberPrefix;
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
