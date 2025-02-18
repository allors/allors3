// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class PurchaseReturnExistShipmentNumberRule : Rule
    {
        public PurchaseReturnExistShipmentNumberRule(MetaPopulation m) : base(m, new Guid("489a2e64-c082-4255-ae70-fe3351c12542")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseReturn.RolePattern(v => v.ShipFromParty),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseReturn>())
            {
                if (!@this.ExistShipmentNumber && @this.ShipFromParty is InternalOrganisation shipFromParty)
                {
                    var year = @this.Strategy.Transaction.Now().Year;
                    @this.ShipmentNumber = shipFromParty.NextPurchaseReturnNumber(year);

                    var fiscalYearInternalOrganisationSequenceNumbers = shipFromParty.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = ((InternalOrganisation)@this.ShipFromParty).PurchaseReturnSequence.IsEnforcedSequence ? ((InternalOrganisation)@this.ShipFromParty).PurchaseReturnNumberPrefix : fiscalYearInternalOrganisationSequenceNumbers.PurchaseReturnNumberPrefix;
                    @this.SortableShipmentNumber = @this.Transaction().GetSingleton().SortableNumber(prefix, @this.ShipmentNumber, year.ToString());
                }
            }
        }
    }
}
