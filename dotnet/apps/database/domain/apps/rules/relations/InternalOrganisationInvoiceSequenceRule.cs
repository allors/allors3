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
    using Derivations.Rules;

    public class InternalOrganisationInvoiceSequenceRule : Rule
    {
        public InternalOrganisationInvoiceSequenceRule(MetaPopulation m) : base(m, new Guid("c0065df6-312a-4871-825f-2d43afb6c14f")) =>
            this.Patterns = new Pattern[]
            {
                m.InternalOrganisation.RolePattern(v => v.IsInternalOrganisation),
                m.InternalOrganisation.RolePattern(v => v.InvoiceSequence),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<InternalOrganisation>())
            {
                var organisation = (Organisation)@this;
                if (organisation.IsInternalOrganisation)
                {
                    if (@this.InvoiceSequence != new InvoiceSequences(@this.Strategy.Transaction).RestartOnFiscalYear)
                    {
                        if (!@this.ExistPurchaseInvoiceNumberCounter)
                        {
                            @this.PurchaseInvoiceNumberCounter = new CounterBuilder(@this.Strategy.Transaction).Build();
                        }

                        if (!@this.ExistPurchaseOrderNumberCounter)
                        {
                            @this.PurchaseOrderNumberCounter = new CounterBuilder(@this.Strategy.Transaction).Build();
                        }
                    }
                }
            }
        }
    }
}
