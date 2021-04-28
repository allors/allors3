// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;

    public class GoodProductIdentificationsRule : Rule
    {
        public GoodProductIdentificationsRule(MetaPopulation m) : base(m, new Guid("868a05f7-326b-47f5-8275-668f0948c7aa")) =>
            this.Patterns = new Pattern[]
            {
                m.Good.RolePattern(v => v.ProductIdentifications),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Good>())
            {
                var identifications = @this.ProductIdentifications;
                identifications.Filter.AddEquals(this.M.ProductIdentification.ProductIdentificationType, new ProductIdentificationTypes(@this.Strategy.Transaction).Good);
                var goodIdentification = identifications.FirstOrDefault();

                @this.ProductNumber = goodIdentification?.Identification;

                if (!@this.ExistProductIdentifications)
                {
                    cycle.Validation.AssertExists(@this, this.M.Good.ProductIdentifications);
                }
            }
        }
    }
}
