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

    public class SalesInvoiceAccountingTransactionRule : Rule
    {
        public SalesInvoiceAccountingTransactionRule(MetaPopulation m) : base(m, new Guid("28b2c9e4-6ed6-46cc-807a-c1e522e65dae")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesInvoice.RolePattern(v => v.PreviousSalesInvoiceState),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            // Parallel to SalesInvoiceInvoiceNumberRule
            foreach (var @this in matches.Cast<SalesInvoice>()
                .Where(v => v.SalesInvoiceState.IsNotPaid
                        && v.ExistPreviousSalesInvoiceState
                        && v.PreviousSalesInvoiceState.IsReadyForPosting))
            {
                @this.DeriveSalesInvoiceAccountingTransaction(validation);
            }
        }
    }

    public static class SalesInvoiceAccountingTransactionRuleExtensions
    {
        public static void DeriveSalesInvoiceAccountingTransaction(this SalesInvoice @this, IValidation validation)
        {
            if (@this.BilledFrom.ExportAccounting)
            {
                var accountSettings = @this.BilledFrom.SettingsForAccounting;
                var debit = new BalanceSides(@this.Transaction()).Debit;
                var credit = new BalanceSides(@this.Transaction()).Credit;

                var accountingTransaction = new AccountingTransactionBuilder(@this.Transaction())
                    .WithAccountingTransactionType(new AccountingTransactionTypes(@this.Transaction()).SalesInvoice)
                    .WithInternalOrganisation(@this.BilledFrom)
                    .WithFromParty(@this.BilledFrom)
                    .WithToParty(@this.BillToCustomer)
                    .WithInvoice(@this)
                    .Build();

                accountingTransaction.AddAccountingTransactionDetail(
                    new AccountingTransactionDetailBuilder(@this.Transaction())
                    .WithGeneralLedgerAccount(accountSettings.AccountsReceivable)
                    .WithBalanceSide(@this.SalesInvoiceType.IsSalesInvoice ? debit : credit)
                    .WithAmount(@this.GrandTotalInPreferredCurrency)
                    .Build()
                );

                foreach (var invoiceItemType in @this.SalesInvoiceItems.Select(v => v.InvoiceItemType).Distinct())
                {
                    var totalExVat = @this.SalesInvoiceItems.Where(v => v.InvoiceItemType.Equals(invoiceItemType)).Sum(v => v.TotalExVat);
                    var totalExVatInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(totalExVat, @this.InvoiceDate, @this.DerivedCurrency, @this.BilledFrom.PreferredCurrency), 2);

                    accountingTransaction.AddAccountingTransactionDetail(
                        new AccountingTransactionDetailBuilder(@this.Transaction())
                        .WithGeneralLedgerAccount(accountSettings.SettingsForInvoiceItemTypes.FirstOrDefault(v => v.InvoiceItemType.Equals(invoiceItemType)).SalesGeneralLedgerAccount)
                        .WithBalanceSide(@this.SalesInvoiceType.IsSalesInvoice ? credit : debit)
                        .WithAmount(totalExVatInPreferredCurrency)
                        .Build()
                    );
                }

                if (@this.TotalVat != 0)
                {
                    foreach (var vatRate in @this.SalesInvoiceItems.Select(v => v.VatRate).Distinct())
                    {
                        var totalVat = @this.SalesInvoiceItems.Where(v => v.VatRate.Equals(vatRate)).Sum(v => v.TotalVat);
                        var totalVatInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(totalVat, @this.InvoiceDate, @this.DerivedCurrency, @this.BilledFrom.PreferredCurrency), 2);

                        accountingTransaction.AddAccountingTransactionDetail(
                            new AccountingTransactionDetailBuilder(@this.Transaction())
                            .WithGeneralLedgerAccount(accountSettings.SettingsForVatRates.FirstOrDefault(v => v.VatRate.Equals(vatRate)).VatPayableAccount)
                            .WithBalanceSide(@this.SalesInvoiceType.IsSalesInvoice ? credit : debit)
                            .WithAmount(totalVatInPreferredCurrency)
                            .Build()
                        );
                    }
                }

                if (@this.TotalIrpf != 0)
                {
                    foreach (var irpfRate in @this.SalesInvoiceItems.Select(v => v.IrpfRate).Distinct())
                    {
                        var totalIrpf = @this.SalesInvoiceItems.Where(v => v.IrpfRate.Equals(irpfRate)).Sum(v => v.TotalIrpf);
                        var totalIrpfInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(totalIrpf, @this.InvoiceDate, @this.DerivedCurrency, @this.BilledFrom.PreferredCurrency), 2);

                        accountingTransaction.AddAccountingTransactionDetail(
                            new AccountingTransactionDetailBuilder(@this.Transaction())
                            .WithGeneralLedgerAccount(accountSettings.SettingsForIrpfRates.FirstOrDefault(v => v.IrpfRate.Equals(irpfRate)).IrpfReceivableAccount)
                            .WithBalanceSide(@this.SalesInvoiceType.IsSalesInvoice ? debit : credit)
                            .WithAmount(totalIrpfInPreferredCurrency)
                            .Build()
                        );
                    }
                }

                var totalDebit = accountingTransaction.AccountingTransactionDetails.Where(v => v.BalanceSide.Equals(debit)).Sum(v => v.Amount);
                var totalCredit = accountingTransaction.AccountingTransactionDetails.Where(v => v.BalanceSide.Equals(credit)).Sum(v => v.Amount);

                if (totalDebit - totalCredit < 0)
                {
                    accountingTransaction.AddAccountingTransactionDetail(
                        new AccountingTransactionDetailBuilder(@this.Transaction())
                        .WithGeneralLedgerAccount(accountSettings.CalculationDifferences)
                        .WithBalanceSide(debit)
                        .WithAmount((totalDebit - totalCredit) * -1)
                        .Build()
                    );
                }
                else if (totalDebit - totalCredit > 0)
                {
                    accountingTransaction.AddAccountingTransactionDetail(
                        new AccountingTransactionDetailBuilder(@this.Transaction())
                        .WithGeneralLedgerAccount(accountSettings.CalculationDifferences)
                        .WithBalanceSide(credit)
                        .WithAmount(totalDebit - totalCredit)
                        .Build()
                    );
                }
            }
        }
    }
}
