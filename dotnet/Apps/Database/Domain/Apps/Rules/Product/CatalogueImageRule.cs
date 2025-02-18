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

    public class CatalogueImageRule : Rule
    {
        public CatalogueImageRule(MetaPopulation m) : base(m, new Guid("005645b4-b150-4edc-a9a6-034774db7b08")) =>
            this.Patterns = new[]
            {
                m.Catalogue.RolePattern(v => v.CatalogueImage),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Catalogue>())
            {
                if (!@this.ExistCatalogueImage)
                {
                    @this.CatalogueImage = @this.Strategy.Transaction.GetSingleton().Settings.NoImageAvailableImage;
                }
            }
        }
    }
}
