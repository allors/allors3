// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class SupplierRelationshipDerivation : DomainDerivation
    {
        public SupplierRelationshipDerivation(M m) : base(m, new Guid("D0B8E2E4-3A11-474A-99FC-B39E4DDAD6E5")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SupplierRelationship, m.SupplierRelationship.Supplier),
                new RolePattern(m.SupplierRelationship, m.SupplierRelationship.InternalOrganisation),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SupplierRelationship>())
            {
                @this.Parties = new Party[] { @this.Supplier, @this.InternalOrganisation };
            }
        }
    }
}
