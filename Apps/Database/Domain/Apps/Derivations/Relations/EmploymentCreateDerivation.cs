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

    public class EmploymentCreateDerivation : DomainDerivation
    {
        public EmploymentCreateDerivation(M m) : base(m, new Guid("4940f8a2-7a7e-440e-bcee-272a1cccb697")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.Employment.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<Employment>())
            {
                // TODO: Don't extent for InternalOrganisations
                var internalOrganisations = new Organisations(@this.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

                if (!@this.ExistEmployer && internalOrganisations.Length == 1)
                {
                    @this.Employer = internalOrganisations.First();
                }
            }
        }
    }
}
