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

    public class InternalOrganisationRequestSequenceRule : Rule
    {
        public InternalOrganisationRequestSequenceRule(MetaPopulation m) : base(m, new Guid("95ef5957-9b9c-4692-a135-433a011faa40")) =>
            this.Patterns = new Pattern[]
            {
                m.InternalOrganisation.RolePattern(v => v.RequestSequence),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<InternalOrganisation>())
            {
                //Altijd
                var organisation = (Organisation)@this;
                if (organisation.IsInternalOrganisation)
                {
                    if (@this.RequestSequence != new RequestSequences(@this.Strategy.Transaction).RestartOnFiscalYear && !@this.ExistRequestNumberCounter)
                    {
                        @this.RequestNumberCounter = new CounterBuilder(@this.Strategy.Transaction).Build();
                    }
                }
            }
        }
    }
}
