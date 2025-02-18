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

    public class SingletonLocalesRule : Rule
    {
        public SingletonLocalesRule(MetaPopulation m) : base(m, new Guid("1a6c3dcc-0ddb-4788-8167-791cfd973b19")) =>
            this.Patterns = new[]
            {
                m.Singleton.RolePattern(v => v.DefaultLocale),
                m.Singleton.RolePattern(v => v.AdditionalLocales),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Singleton>())
            {
                @this.Locales = @this.AdditionalLocales;
                @this.AddLocale(@this.DefaultLocale);
            }
        }
    }
}
