// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class SingletonLocalesDerivation : DomainDerivation
    {
        public SingletonLocalesDerivation(M m) : base(m, new Guid("1a6c3dcc-0ddb-4788-8167-791cfd973b19")) =>
            this.Patterns = new[]
            {
                new ChangedRolePattern(this.M.Singleton.DefaultLocale),
                new ChangedRolePattern(this.M.Singleton.AdditionalLocales)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var singleton in matches.Cast<Singleton>())
            {
                singleton.Locales = singleton.AdditionalLocales;
                singleton.AddLocale(singleton.DefaultLocale);
            }
        }
    }
}
