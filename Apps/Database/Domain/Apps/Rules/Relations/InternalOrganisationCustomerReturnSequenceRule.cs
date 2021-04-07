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

    public class InternalOrganisationCustomerReturnSequenceRule : Rule
    {
        public InternalOrganisationCustomerReturnSequenceRule(MetaPopulation m) : base(m, new Guid("fbef3c80-a576-4a5c-9dbf-fd7cf2cc967c")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.InternalOrganisation, m.InternalOrganisation.CustomerReturnSequence),
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
                }
            }
        }
    }
}
