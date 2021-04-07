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

    public class PurchaseReturnExistShipmentNumberRule : Rule
    {
        public PurchaseReturnExistShipmentNumberRule(MetaPopulation m) : base(m, new Guid("489a2e64-c082-4255-ae70-fe3351c12542")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PurchaseReturn, m.PurchaseReturn.Store),
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
            }
        }
    }
}
