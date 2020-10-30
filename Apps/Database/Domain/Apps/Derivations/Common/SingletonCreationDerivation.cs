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

    public class SingletonCreationDerivation : DomainDerivation
    {
        public SingletonCreationDerivation(M m) : base(m, new Guid("5195dc97-6005-4a2c-b6ae-041f46969d3b")) =>
            this.Patterns = new[]
            {
                new CreatedPattern(this.M.Singleton.Class)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Singleton>())
            {
                if (!@this.ExistLogoImage)
                {
                    @this.LogoImage = new MediaBuilder(@this.Strategy.Session).WithInFileName("allors.png").WithInData(@this.GetResourceBytes("allors.png")).Build();
                }
            }
        }
    }
}
