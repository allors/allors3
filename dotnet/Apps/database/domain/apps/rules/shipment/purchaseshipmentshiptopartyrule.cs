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
    using Derivations.Rules;

    public class PurchaseShipmentShipToPartyRule : Rule
    {
        public PurchaseShipmentShipToPartyRule(MetaPopulation m) : base(m, new Guid("89A2FB27-6839-40D4-AFAB-79E25259B1C8")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseShipment.RolePattern(v => v.ShipToParty),
                m.PurchaseShipment.RolePattern(v => v.ShipToAddress),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseShipment>())
            {
                if (!@this.ExistShipmentNumber && @this.ShipToParty is InternalOrganisation shipToParty)
                {
                    var year = @this.Strategy.Transaction.Now().Year;
                    @this.ShipmentNumber = shipToParty.NextPurchaseShipmentNumber(year);

                    var fiscalYearInternalOrganisationSequenceNumbers = shipToParty.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = ((InternalOrganisation)@this.ShipToParty).CustomerShipmentSequence.IsEnforcedSequence ? ((InternalOrganisation)@this.ShipToParty).PurchaseShipmentNumberPrefix : fiscalYearInternalOrganisationSequenceNumbers.PurchaseShipmentNumberPrefix;
                    @this.SortableShipmentNumber = @this.Transaction().GetSingleton().SortableNumber(prefix, @this.ShipmentNumber, year.ToString());
                }

                @this.ShipToAddress ??= @this.ShipToParty?.ShippingAddress ?? @this.ShipToParty?.GeneralCorrespondence as PostalAddress;
            }
        }
    }
}
