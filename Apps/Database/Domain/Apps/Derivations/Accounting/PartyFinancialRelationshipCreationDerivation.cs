// <copyright file="PartyFinancialRelationshipCreationDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class PartyFinancialRelationshipCreationDerivation : DomainDerivation
    {
        public PartyFinancialRelationshipCreationDerivation(M m) : base(m, new Guid("8403E05C-8C82-47E9-8649-748294FC8463")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(m.PartyFinancialRelationship.Class)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PartyFinancialRelationship>())
            {
                if (@this != null)
                {
                    var party = @this.FinancialParty;

                    var internalOrganisations = new Organisations(@this.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

                    if (!@this.ExistInternalOrganisation && internalOrganisations.Count() == 1)
                    {
                        @this.InternalOrganisation = internalOrganisations.First();
                    }

                    @this.Parties = new Party[] { @this.FinancialParty, @this.InternalOrganisation };
                }
            }
        }
    }
}
