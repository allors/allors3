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

    public class PersonDisplayNameRule : Rule
    {
        public PersonDisplayNameRule(MetaPopulation m) : base(m, new Guid("0df200cd-bc85-4bb4-9d84-939dc2b47492")) =>
            this.Patterns = new Pattern[]
            {
                m.Person.RolePattern(v => v.FirstName),
                m.Person.RolePattern(v => v.MiddleName),
                m.Person.RolePattern(v => v.LastName),
                m.Person.RolePattern(v => v.UserName),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Person>())
            {
                @this.Strategy.Transaction.Prefetch(@this.PrefetchPolicy, @this);
                @this.DerivePersonDisplayName(validation);
            }
        }
    }

    public static class PersonDisplayNameRuleExtensions
    {
        public static void DerivePersonDisplayName(this Person @this, IValidation validation)
        {
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

            if (builder.Length == 0 && @this.ExistUserName)
            {
                builder.Append($"[{@this.UserName}]");
            }

            if (builder.Length == 0 && !@this.ExistUserName)
            {
                builder.Append("N/A");
            }

            @this.DisplayName = builder.ToString();
        }
    }
}
