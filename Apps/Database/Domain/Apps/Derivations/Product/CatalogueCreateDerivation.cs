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

    public class CatalogueCreateDerivation : DomainDerivation
    {
        public CatalogueCreateDerivation(M m) : base(m, new Guid("f337fa89-5306-44f7-802e-7d48ed6ffbdf")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.Catalogue.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<Catalogue>())
            {
                if (!@this.ExistCatScope)
                {
                    @this.CatScope = new CatScopes(@this.Strategy.Session).Public;
                }
            }
        }
    }
}
