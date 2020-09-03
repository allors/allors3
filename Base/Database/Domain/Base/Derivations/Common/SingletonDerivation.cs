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

    public class SingletonDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("5195dc97-6005-4a2c-b6ae-041f46969d3b");

        public IEnumerable<Pattern> Patterns { get; } = new[] { new CreatedPattern(M.Singleton.Class) };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var singleton in matches.Cast<Singleton>())
            {
                if (!singleton.ExistLogoImage)
                {
                    singleton.LogoImage = new MediaBuilder(singleton.Strategy.Session).WithInFileName("allors.png").WithInData(singleton.GetResourceBytes("allors.png")).Build();
                }

                singleton.Locales = singleton.AdditionalLocales;
                singleton.AddLocale(singleton.DefaultLocale);
            }
        }
    }
}
