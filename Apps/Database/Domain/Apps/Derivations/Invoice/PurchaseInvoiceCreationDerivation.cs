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

    public class PurchaseInvoiceCreationDerivation : DomainDerivation
    {
        public PurchaseInvoiceCreationDerivation(M m) : base(m, new Guid("5ea3d653-a671-4cb8-b7a5-35099da64434")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(m.PurchaseInvoice.Class)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseInvoice>())
            {
                if (!@this.ExistPurchaseInvoiceState)
                {
                    @this.PurchaseInvoiceState = new PurchaseInvoiceStates(@this.Strategy.Session).Created;
                }

                if (!@this.ExistInvoiceDate)
                {
                    @this.InvoiceDate = @this.Session().Now();
                }

                if (!@this.ExistEntryDate)
                {
                    @this.EntryDate = @this.Session().Now();
                }
            }
        }
    }
}
