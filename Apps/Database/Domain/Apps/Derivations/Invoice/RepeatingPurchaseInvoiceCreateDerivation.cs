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

    public class RepeatingPurchaseInvoiceCreateDerivation : DomainDerivation
    {
        public RepeatingPurchaseInvoiceCreateDerivation(M m) : base(m, new Guid("8fa2cac1-d133-41cc-8911-ca31fd61dd13")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(m.RepeatingPurchaseInvoice.Class)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<RepeatingPurchaseInvoice>())
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
