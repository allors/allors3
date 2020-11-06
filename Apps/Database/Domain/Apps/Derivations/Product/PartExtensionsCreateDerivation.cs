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

    public class PartExtensionsCreateDerivation : DomainDerivation
    {
        public PartExtensionsCreateDerivation(M m) : base(m, new Guid("9d03e954-9395-4329-a9a3-57078b813973")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.Part.Interface),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<Part>())
            {
                if (!@this.ExistPartWeightedAverage)
                {
                    @this.PartWeightedAverage = new PartWeightedAverageBuilder(@this.Session()).Build();
                }
            }
        }
    }
}
