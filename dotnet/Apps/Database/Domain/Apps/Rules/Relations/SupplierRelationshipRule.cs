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

    public class SupplierRelationshipRule : Rule
    {
        public SupplierRelationshipRule(MetaPopulation m) : base(m, new Guid("D0B8E2E4-3A11-474A-99FC-B39E4DDAD6E5")) =>
            this.Patterns = new Pattern[]
            {
                m.SupplierRelationship.RolePattern(v => v.Supplier),
                m.SupplierRelationship.RolePattern(v => v.InternalOrganisation),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SupplierRelationship>())
            {
                @this.Parties = new Party[] { @this.Supplier, @this.InternalOrganisation };

                if (@this.ExistSupplier)
                {
                    @this.Supplier.DerivationTrigger = Guid.NewGuid();
                }
            }
        }
    }
}
