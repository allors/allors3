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

    public class OrganisationRollupDerivation : DomainDerivation
    {
        public OrganisationRollupDerivation(M m) : base(m, new Guid("F34E1F40-B1DD-4F0D-A87C-78F44ACF8512")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.OrganisationRollUp.Parent),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<OrganisationRollUp>())
            {
                @this.Parties = new Party[] { @this.Child, @this.Parent };

                if (!@this.ExistParent && !@this.ExistChild)
                {
                    // TODO: Move Delete
                    @this.Delete();
                }
            }
        }
    }
}
