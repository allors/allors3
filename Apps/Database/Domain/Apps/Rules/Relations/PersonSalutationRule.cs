// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Meta;
    using Database.Derivations;

    public class PersonSalutationRule : Rule
    {
        public PersonSalutationRule(MetaPopulation m) : base(m, new Guid("d418c1c6-4113-4d82-af35-e0cc46a08f8a")) =>
            this.Patterns = new Pattern[]
            {
                m.Person.RolePattern(v => v.Salutation),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Person>())
            {
                @this.Strategy.Transaction.Prefetch(@this.PrefetchPolicy);

                if (@this.ExistSalutation
                    && (@this.Salutation.Equals(new Salutations(@this.Transaction()).Mr)
                        || @this.Salutation.Equals(new Salutations(@this.Transaction()).Dr)))
                {
                    @this.Gender = new GenderTypes(@this.Transaction()).Male;
                }

                if (@this.ExistSalutation
                    && (@this.Salutation.Equals(new Salutations(@this.Transaction()).Mrs)
                        || @this.Salutation.Equals(new Salutations(@this.Transaction()).Ms)
                        || @this.Salutation.Equals(new Salutations(@this.Transaction()).Mme)))
                {
                    @this.Gender = new GenderTypes(@this.Transaction()).Female;
                }
            }
        }
    }
}
