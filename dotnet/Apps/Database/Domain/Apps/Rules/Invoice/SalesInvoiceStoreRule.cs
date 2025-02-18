// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class SalesInvoiceStoreRule : Rule
    {
        public SalesInvoiceStoreRule(MetaPopulation m) : base(m, new Guid("01d1055a-d116-44fb-8f26-8e4062c216a0")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesInvoice.RolePattern(v => v.BilledFrom),
            m.SalesInvoice.RolePattern(v => v.Store),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                if (!@this.ExistStore && @this.ExistBilledFrom)
                {
                    var stores = @this.BilledFrom.StoresWhereInternalOrganisation;
                    @this.Store = stores.FirstOrDefault();
                }
            }
        }
    }
}
