// <copyright file="NonUnifiedPartDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Derivations.Rules;

    public class NonUnifiedPartProductIdentificationsRule : Rule
    {
        public NonUnifiedPartProductIdentificationsRule(MetaPopulation m) : base(m, new Guid("6178ee81-e432-48e3-97a3-b25ee4d36c3c")) =>
            this.Patterns = new Pattern[]
            {
                m.NonUnifiedPart.RolePattern(v => v.ProductIdentifications),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<NonUnifiedPart>())
            {
                var part = new ProductIdentificationTypes(@this.Strategy.Transaction).Part;
                var partIdentification = @this.ProductIdentifications.FirstOrDefault(v => Equals(part, v.ProductIdentificationType));

                @this.ProductNumber = partIdentification?.Identification;
            }
        }
    }
}
