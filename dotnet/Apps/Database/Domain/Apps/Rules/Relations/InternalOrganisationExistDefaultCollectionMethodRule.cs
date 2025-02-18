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

    public class InternalOrganisationExistDefaultCollectionMethodRule : Rule
    {
        public InternalOrganisationExistDefaultCollectionMethodRule(MetaPopulation m) : base(m, new Guid("1f351066-cb01-4cbe-918e-6fe04c43ffd7")) =>
            this.Patterns = new Pattern[]
            {
                m.InternalOrganisation.RolePattern(v => v.IsInternalOrganisation),
                m.InternalOrganisation.RolePattern(v => v.DefaultCollectionMethod),
                m.InternalOrganisation.RolePattern(v => v.AssignedActiveCollectionMethods),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<InternalOrganisation>())
            {
                var organisation = (Organisation)@this;
                if (organisation.IsInternalOrganisation)
                {
                    if (!@this.ExistDefaultCollectionMethod && @this.Strategy.Transaction.Extent<PaymentMethod>().Count == 1)
                    {
                        @this.DefaultCollectionMethod = @this.Strategy.Transaction.Extent<PaymentMethod>().First;
                    }

                    @this.DerivedActiveCollectionMethods = @this.AssignedActiveCollectionMethods;
                    @this.AddDerivedActiveCollectionMethod(@this.DefaultCollectionMethod);
                }
            }
        }
    }
}
