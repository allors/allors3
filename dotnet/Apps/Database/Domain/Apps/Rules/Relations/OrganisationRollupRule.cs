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

    public class OrganisationRollupRule : Rule
    {
        public OrganisationRollupRule(MetaPopulation m) : base(m, new Guid("F34E1F40-B1DD-4F0D-A87C-78F44ACF8512")) =>
            this.Patterns = new Pattern[]
            {
                m.OrganisationRollUp.RolePattern(v => v.Parent),
                m.OrganisationRollUp.RolePattern(v => v.Child),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
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
