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

    public class FiscalYearInvoiceNumberCreationDerivation : DomainDerivation
    {
        public FiscalYearInvoiceNumberCreationDerivation(M m) : base(m, new Guid("f2c9f4db-68f2-45fe-b002-4cc6ac2a2b17")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(m.FiscalYearInvoiceNumber.Class)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<FiscalYearInvoiceNumber>())
            {
                if (!@this.ExistNextSalesInvoiceNumber)
                {
                    @this.NextSalesInvoiceNumber = 1;
                }

                if (!@this.ExistNextCreditNoteNumber)
                {
                    @this.NextCreditNoteNumber = 1;
                }
            }
        }
    }
}
