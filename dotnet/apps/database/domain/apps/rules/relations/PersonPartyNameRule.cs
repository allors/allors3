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

    public class PersonPartyNameRule : Rule
    {
        public PersonPartyNameRule(MetaPopulation m) : base(m, new Guid("0df200cd-bc85-4bb4-9d84-939dc2b47492")) =>
            this.Patterns = new Pattern[]
            {
                m.Person.RolePattern(v => v.FirstName),
                m.Person.RolePattern(v => v.MiddleName),
                m.Person.RolePattern(v => v.LastName),
                m.Person.RolePattern(v => v.UserName),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Person>())
            {
                @this.Strategy.Transaction.Prefetch(@this.PrefetchPolicy);

                var builder = new StringBuilder();

                if (@this.ExistFirstName)
                {
                    builder.Append(@this.FirstName);
                }

                if (@this.ExistMiddleName)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(" ");
                    }

                    builder.Append(@this.MiddleName);
                }

                if (@this.ExistLastName)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(" ");
                    }

                    builder.Append(@this.LastName);
                }

                if (builder.Length == 0)
                {
                    builder.Append($"[{@this.UserName}]");
                }

                @this.PartyName = builder.ToString();
            }
        }
    }
}
