// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Allors.Meta;
    using Derivations;
    using Resources;

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
                var internalOrganisations = new Organisations(@this.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

                if (!@this.ExistInternalOrganisation && internalOrganisations.Count() == 1)
                {
                    @this.InternalOrganisation = internalOrganisations.First();
                }
            }
        }
    }
}
