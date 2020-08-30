// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class SingletonCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdSingletons = changeSet.Created.Select(v=>v.GetObject()).OfType<Singleton>();

                foreach(var singleton in createdSingletons)
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

        public static void SingletonRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("2048646b-849f-4b36-8bb5-788dc2cfeb0f")] = new SingletonCreationDerivation();
        }
    }
}
