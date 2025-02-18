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

    public class AccountingTransactionInvoiceNumberRule : Rule
    {
        public AccountingTransactionInvoiceNumberRule(MetaPopulation m) : base(m, new Guid("70d2451e-d7f7-448d-915c-b88e56810902")) =>
            this.Patterns = new Pattern[]
            {
                m.AccountingTransaction.RolePattern(v => v.Invoice),
                m.Invoice.RolePattern(v => v.InvoiceNumber, v => v.AccountingTransactionsWhereInvoice.ObjectType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<AccountingTransaction>())
            {
                @this.DeriveAccountingTransactionInvoiceNumber(validation);
            }
        }
    }

    public static class AccountingTransactionInvoiceNumberRuleExtensions
    {
        public static void DeriveAccountingTransactionInvoiceNumber(this AccountingTransaction @this, IValidation validation) => @this.InvoiceNumber = @this.Invoice.InvoiceNumber;
    }
}
