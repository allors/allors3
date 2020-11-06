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

    public class SupplierRelationshipCreateDerivation : DomainDerivation
    {
        public SupplierRelationshipCreateDerivation(M m) : base(m, new Guid("b38a625d-228d-481a-8508-7c19174394b0")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.SubContractorRelationship.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<SubContractorRelationship>())
            {
                // TODO: Don't extent for InternalOrganisations
                var internalOrganisations = new Organisations(@this.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

                if (!@this.ExistContractor && internalOrganisations.Count() == 1)
                {
                    @this.Contractor = internalOrganisations.First();
                }
            }
        }
    }
}
