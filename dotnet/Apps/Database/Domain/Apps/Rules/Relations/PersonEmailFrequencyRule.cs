// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class PersonEmailFrequencyRule : Rule
    {
        public PersonEmailFrequencyRule(MetaPopulation m) : base(m, new Guid("c2815b49-0c16-4758-9cb0-ce328df72fbb")) =>
            this.Patterns = new Pattern[]
            {
                m.Person.RolePattern(v => v.EmailFrequency),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Person>())
            {
                @this.DerivePersonEmailFrequency(validation);
            }
        }
    }

    public static class PersonEmailFrequencyRuleExtensions
    {
        public static void DerivePersonEmailFrequency(this Person @this, IValidation validation)
        {
            var builder = new StringBuilder();
            var transaction = @this.Strategy.Transaction;

            if (@this.Strategy.IsNewInTransaction && !@this.ExistEmailFrequency)
            {
                @this.EmailFrequency = new EmailFrequencies(transaction).NoEmail;
            }
        }
    }
}
