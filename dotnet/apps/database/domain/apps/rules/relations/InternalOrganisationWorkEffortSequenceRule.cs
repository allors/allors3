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

    public class InternalOrganisationWorkEffortSequenceRule : Rule
    {
        public InternalOrganisationWorkEffortSequenceRule(MetaPopulation m) : base(m, new Guid("ec9af113-6d12-49f4-b0d6-bbf5e500f44b")) =>
            this.Patterns = new Pattern[]
            {
                m.InternalOrganisation.RolePattern(v => v.WorkEffortSequence),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<InternalOrganisation>())
            {
                //Altijd
                var organisation = (Organisation)@this;
                if (organisation.IsInternalOrganisation)
                {
                    if (@this.WorkEffortSequence != new WorkEffortSequences(@this.Strategy.Transaction).RestartOnFiscalYear && !@this.ExistWorkEffortNumberCounter)
                    {
                        @this.WorkEffortNumberCounter = new CounterBuilder(@this.Strategy.Transaction).Build();
                    }
                }
            }
        }
    }
}
