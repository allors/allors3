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

    public class DropShipmentExistShipmentNumberRule : Rule
    {
        public DropShipmentExistShipmentNumberRule(MetaPopulation m) : base(m, new Guid("f3fc8882-2989-4f1c-9ad3-1994955db19c")) =>
            this.Patterns = new Pattern[]
            {
                m.DropShipment.RolePattern(v => v.ShipFromParty),
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
            }
        }
    }
}
