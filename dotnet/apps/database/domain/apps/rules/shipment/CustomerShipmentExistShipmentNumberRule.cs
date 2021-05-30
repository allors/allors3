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
    using Derivations.Rules;

    public class CustomerShipmentExistShipmentNumberRule : Rule
    {
        public CustomerShipmentExistShipmentNumberRule(MetaPopulation m) : base(m, new Guid("efe70315-7c14-4f81-8bc5-0c54871146e3")) =>
            this.Patterns = new Pattern[]
            {
                m.CustomerShipment.RolePattern(v => v.Store),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CustomerShipment>())
            {
                if (!@this.ExistShipmentNumber && @this.ExistStore)
                {
                    var year = @this.Transaction().Now().Year;
                    @this.ShipmentNumber = @this.Store.NextCustomerShipmentNumber(year);

                    var fiscalYearStoreSequenceNumbers = @this.Store.FiscalYearsStoreSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = ((InternalOrganisation)@this.ShipFromParty).CustomerShipmentSequence.IsEnforcedSequence ? @this.Store.CustomerShipmentNumberPrefix : fiscalYearStoreSequenceNumbers.CustomerShipmentNumberPrefix;
                    @this.SortableShipmentNumber = @this.Transaction().GetSingleton().SortableNumber(prefix, @this.ShipmentNumber, year.ToString());
                }
            }
        }
    }
}
