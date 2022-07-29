// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

    public class DataRule : Rule
    {
        public DataRule(MetaPopulation m) : base(m, new Guid("B3CADA5C-B844-40BF-82B9-CF4EC41AF198")) =>
            this.Patterns = new Pattern[]
            {
                m.Data.RolePattern(v=>v.AutocompleteAssignedFilter),
                m.Data.RolePattern(v=>v.AutocompleteAssignedOptions),
                m.Data.RolePattern(v=>v.SelectAssigned)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Data>())
            {
                @this.AutocompleteDerivedFilter = @this.AutocompleteAssignedFilter;
                @this.AutocompleteDerivedOptions = @this.AutocompleteAssignedOptions;
                @this.SelectDerived = @this.SelectAssigned;
            }
        }
    }
}
