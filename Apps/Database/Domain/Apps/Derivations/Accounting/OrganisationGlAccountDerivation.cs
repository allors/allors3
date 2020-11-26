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

    public class OrganisationGlAccountDerivation : DomainDerivation
    {
        public OrganisationGlAccountDerivation(M m) : base(m, new Guid("df5c427e-a286-4495-a197-ba3432312276")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.OrganisationGlAccount.InternalOrganisation),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<OrganisationGlAccount>())
            {
            }
        }
    }
}
