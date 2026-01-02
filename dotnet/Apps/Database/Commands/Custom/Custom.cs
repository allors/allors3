// <copyright file="Custom.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System.Linq;
    using Allors.Database.Derivations;
    using Allors.Database.Domain;
    using Allors.Database.Services;
    using McMaster.Extensions.CommandLineUtils;

    [Command(Description = "Execute custom code")]
    public class Custom
    {
        public Program Parent { get; set; }

        public int OnExecute(CommandLineApplication app)
        {
            using var transaction = this.Parent.Database.CreateTransaction();

            var m = this.Parent.M;

            //var scheduler = new AutomatedAgents(transaction).System;
            //transaction.Services.Get<IUserService>().User = scheduler;

            // Custom code

            return this.DoSomething();

            // Custom code
            transaction.Derive();
            transaction.Commit();

            return ExitCode.Success;
        }

        private int DoSomething()
        {
            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                var m = this.Parent.M;

                User user = new People(transaction).FindBy(m.Person.UserName, "jane@example.com");

                transaction.Services.Get<IUserService>().User = user;
                var derivation = transaction.Database.Services.Get<IDerivationService>().CreateDerivation(transaction);

                //var acl = new DatabaseAccessControl(user)[@this];
                //var result = acl.CanExecute(m.PurchaseOrder.Return);

                //user.UserEmail = "sender@aa.com";

                //var all = new People(transaction).Extent().ToArray();
                //all[0].UserEmail = "recipient@aa.com";
                //var recipient = all[0];

                //new EmailMessageBuilder(transaction)
                //    .WithDateCreated(transaction.Now().AddDays(-1).Date)
                //    .WithSender(recipient)
                //    .WithRecipient(user)
                //    .WithSubject("hallo2")
                //    .WithBody("body")
                //    .Build();

                var internalOrg = new Organisations(transaction).Extent().First(v => v.IsInternalOrganisation);
                var accountClassification = new GeneralLedgerAccountClassificationBuilder(transaction)
                    .WithName("accountGroup")
                    .WithReferenceNumber("SCode")
                    .WithReferenceCode("001")
                    .WithSortCode("A")
                    .Build();

                var glAccount0001 = new GeneralLedgerAccountBuilder(transaction)
                    .WithReferenceCode("A")
                    .WithSortCode("A")
                    .WithReferenceNumber("0001")
                    .WithName("GeneralLedgerAccount")
                    .WithBalanceType(new BalanceTypes(transaction).Balance)
                    .WithBalanceSide(new BalanceSides(transaction).Debit)
                    .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(transaction).WithDescription("accountType").Build())
                    .WithGeneralLedgerAccountClassification(accountClassification)
                    .Build();

                new OrganisationGlAccountBuilder(transaction)
                    .WithInternalOrganisation(internalOrg)
                    .WithGeneralLedgerAccount(glAccount0001)
                    .Build();

                var glAccount0002 = new GeneralLedgerAccountBuilder(transaction)
                    .WithReferenceCode("A2")
                    .WithSortCode("A2")
                    .WithReferenceNumber("0002")
                    .WithName("Accounts Receivable")
                    .WithBalanceType(new BalanceTypes(transaction).Balance)
                    .WithBalanceSide(new BalanceSides(transaction).Debit)
                    .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(transaction).WithDescription("accountType").Build())
                    .WithGeneralLedgerAccountClassification(accountClassification)
                    .Build();

                new OrganisationGlAccountBuilder(transaction)
                    .WithInternalOrganisation(internalOrg)
                    .WithGeneralLedgerAccount(glAccount0002)
                    .Build();

                var glAccount0003 = new GeneralLedgerAccountBuilder(transaction)
                    .WithReferenceCode("A3")
                    .WithSortCode("A3")
                    .WithReferenceNumber("0003")
                    .WithName("Revenue")
                    .WithBalanceType(new BalanceTypes(transaction).Balance)
                    .WithBalanceSide(new BalanceSides(transaction).Debit)
                    .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(transaction).WithDescription("accountType").Build())
                    .WithGeneralLedgerAccountClassification(accountClassification)
                    .Build();

                new OrganisationGlAccountBuilder(transaction)
                    .WithInternalOrganisation(internalOrg)
                    .WithGeneralLedgerAccount(glAccount0003)
                    .Build();

                var glAccount0004 = new GeneralLedgerAccountBuilder(transaction)
                    .WithReferenceCode("A4")
                    .WithSortCode("A4")
                    .WithReferenceNumber("0004")
                    .WithName("VAT")
                    .WithBalanceType(new BalanceTypes(transaction).Balance)
                    .WithBalanceSide(new BalanceSides(transaction).Debit)
                    .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(transaction).WithDescription("accountType").Build())
                    .WithGeneralLedgerAccountClassification(accountClassification)
                    .Build();

                new OrganisationGlAccountBuilder(transaction)
                    .WithInternalOrganisation(internalOrg)
                    .WithGeneralLedgerAccount(glAccount0004)
                    .Build();

                var glAccount0005 = new GeneralLedgerAccountBuilder(transaction)
                    .WithReferenceCode("A4")
                    .WithSortCode("A4")
                    .WithReferenceNumber("0005")
                    .WithName("CalculationDifferences")
                    .WithBalanceType(new BalanceTypes(transaction).Balance)
                    .WithBalanceSide(new BalanceSides(transaction).Debit)
                    .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(transaction).WithDescription("accountType").Build())
                    .WithGeneralLedgerAccountClassification(accountClassification)
                    .Build();

                new OrganisationGlAccountBuilder(transaction)
                    .WithInternalOrganisation(internalOrg)
                    .WithGeneralLedgerAccount(glAccount0005)
                    .Build();

                foreach (var setting in internalOrg.SettingsForAccounting.SettingsForVatRates)
                {
                    setting.VatReceivableAccount = glAccount0004;
                    setting.VatPayableAccount = glAccount0004;
                }

                foreach (var setting in internalOrg.SettingsForAccounting.SettingsForInvoiceItemTypes)
                {
                    setting.SalesGeneralLedgerAccount = glAccount0003;
                }

                internalOrg.ExportAccounting = true;
                internalOrg.SettingsForAccounting.AccountsReceivable = glAccount0002;
                internalOrg.SettingsForAccounting.CalculationDifferences = glAccount0005;

                transaction.Derive();
                transaction.Commit();
            }

            return ExitCode.Success;
        }
    }
}
