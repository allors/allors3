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

    public class InternalOrganisationQuoteSequenceRule : Rule
    {
        public InternalOrganisationQuoteSequenceRule(MetaPopulation m) : base(m, new Guid("29b66ddd-a8fa-4463-a108-28e65d79a673")) =>
            this.Patterns = new Pattern[]
            {
                m.InternalOrganisation.RolePattern(v => v.QuoteSequence),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<InternalOrganisation>())
            {
                //Altijd
                var organisation = (Organisation)@this;
                if (organisation.IsInternalOrganisation)
                {
                    if (@this.QuoteSequence != new QuoteSequences(@this.Strategy.Transaction).RestartOnFiscalYear && !@this.ExistQuoteNumberCounter)
                    {
                        @this.QuoteNumberCounter = new CounterBuilder(@this.Strategy.Transaction).Build();
                    }
                }
            }
        }
    }
}
