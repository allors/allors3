// <copyright file="SalesInvoiceItemSubTotalItemDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class SalesInvoiceItemSubTotalItemRule : Rule
    {
        public SalesInvoiceItemSubTotalItemRule(MetaPopulation m) : base(m, new Guid("71d5e781-ac75-4b12-9766-960f1ec1df9f")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SalesInvoiceItem, m.SalesInvoiceItem.InvoiceItemType),
                new RolePattern(m.SalesInvoiceItem, m.SalesInvoiceItem.Quantity),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;
            var changeSet = cycle.ChangeSet;

            foreach (var @this in matches.Cast<SalesInvoiceItem>())
            {
                var isSubTotalItem = @this.ExistInvoiceItemType
                                    && (@this.InvoiceItemType.Equals(new InvoiceItemTypes(transaction).ProductItem)
                                       || @this.InvoiceItemType.Equals(new InvoiceItemTypes(transaction).PartItem));

                if (@this.ExistInvoiceItemType && isSubTotalItem && @this.Quantity <= 0)
                {
                    validation.AddError($"{@this}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}");
                }
            }
        }
    }
}
