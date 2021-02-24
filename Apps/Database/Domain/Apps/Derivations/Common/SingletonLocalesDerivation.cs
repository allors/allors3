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

    public class SingletonLocalesDerivation : DomainDerivation
    {
        public SingletonLocalesDerivation(M m) : base(m, new Guid("1a6c3dcc-0ddb-4788-8167-791cfd973b19")) =>
            this.Patterns = new[]
            {
                new AssociationPattern(this.M.Singleton.DefaultLocale),
                new AssociationPattern(this.M.Singleton.AdditionalLocales)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Singleton>())
            {
                @this.Locales = @this.AdditionalLocales;
                @this.AddLocale(@this.DefaultLocale);
            }
        }
    }
}
