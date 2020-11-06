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

    public class CustomerRelationshipCreateDerivation : DomainDerivation
    {
        public CustomerRelationshipCreateDerivation(M m) : base(m, new Guid("77a717bc-d7e1-4f40-9dcb-6bde5503bd62")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.CustomerRelationship.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CustomerRelationship>())
            {
                // TODO: Don't extent for InternalOrganisations
                var internalOrganisations = new Organisations(@this.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

                if (!@this.ExistInternalOrganisation && internalOrganisations.Length == 1)
                {
                    @this.InternalOrganisation = internalOrganisations.First();
                }
            }
        }
    }
}
