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

    public class CustomerReturnExistShipmentNumberRule : Rule
    {
        public CustomerReturnExistShipmentNumberRule(MetaPopulation m) : base(m, new Guid("43adc988-8474-4390-ab26-a7253c21d8c1")) =>
            this.Patterns = new Pattern[]
            {
                m.CustomerReturn.RolePattern(v => v.ShipToParty),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CustomerReturn>())
            {
                if (!@this.ExistShipmentNumber && @this.ShipToParty is InternalOrganisation shipToParty)
                {
                    var year = @this.Strategy.Transaction.Now().Year;
                    @this.ShipmentNumber = shipToParty.NextCustomerReturnNumber(year);

                    var fiscalYearInternalOrganisationSequenceNumbers = shipToParty.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = ((InternalOrganisation)@this.ShipToParty).CustomerShipmentSequence.IsEnforcedSequence ? ((InternalOrganisation)@this.ShipToParty).CustomerReturnNumberPrefix : fiscalYearInternalOrganisationSequenceNumbers.CustomerReturnNumberPrefix;
                    @this.SortableShipmentNumber = @this.Transaction().GetSingleton().SortableNumber(prefix, @this.ShipmentNumber, year.ToString());
                }
            }
        }
    }
}
