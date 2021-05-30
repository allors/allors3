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

    public class InternalOrganisationRule : Rule
    {
        public InternalOrganisationRule(MetaPopulation m) : base(m, new Guid("258A6E3B-7940-4FCC-A33E-AE07C6FBFC32")) =>
            this.Patterns = new Pattern[]
            {
                m.InternalOrganisation.RolePattern(v => v.PurchaseShipmentSequence),
                m.InternalOrganisation.RolePattern(v => v.CustomerReturnSequence),
                m.InternalOrganisation.RolePattern(v => v.IncomingTransferSequence),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<InternalOrganisation>())
            {
                //Altijd
                var organisation = (Organisation)@this;
                if (organisation.IsInternalOrganisation)
                {
                    if (@this.CustomerReturnSequence != new CustomerReturnSequences(@this.Strategy.Transaction).RestartOnFiscalYear && !@this.ExistCustomerReturnNumberCounter)
                    {
                        @this.CustomerReturnNumberCounter = new CounterBuilder(@this.Strategy.Transaction).Build();
                    }

                    if (@this.IncomingTransferSequence != new IncomingTransferSequences(@this.Strategy.Transaction).RestartOnFiscalYear && !@this.ExistIncomingTransferNumberCounter)
                    {
                        @this.IncomingTransferNumberCounter = new CounterBuilder(@this.Strategy.Transaction).Build();
                    }
                }
            }
        }
    }
}
