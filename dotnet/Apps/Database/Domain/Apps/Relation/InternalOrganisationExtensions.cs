// <copyright file="InternalOrganisationExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;
    using Meta;
    using DateTime = System.DateTime;

    public static partial class InternalOrganisationExtensions
    {
        public static void AppsOnInit(this InternalOrganisation @this, ObjectOnInit method)
        {
            var singleton = @this.Transaction().GetSingleton();

            if (@this.IsInternalOrganisation)
            {
                if (!@this.ExistProductQuoteTemplate)
                {
                    @this.ProductQuoteTemplate =
                        singleton.CreateOpenDocumentTemplate<Print.ProductQuoteModel.Model>("ProductQuote.odt",
                            singleton.GetResourceBytes("Templates.ProductQuote.odt"));
                }

                if (!@this.ExistProposalTemplate)
                {
                    @this.ProposalTemplate =
                        singleton.CreateOpenDocumentTemplate<Print.ProposalModel.Model>("Proposal.odt",
                            singleton.GetResourceBytes("Templates.Proposal.odt"));
                }

                if (!@this.ExistSalesOrderTemplate)
                {
                    @this.SalesOrderTemplate =
                        singleton.CreateOpenDocumentTemplate<Print.SalesOrderModel.Model>("SalesOrder.odt",
                            singleton.GetResourceBytes("Templates.SalesOrder.odt"));
                }

                if (!@this.ExistPurchaseOrderTemplate)
                {
                    @this.PurchaseOrderTemplate =
                        singleton.CreateOpenDocumentTemplate<Print.PurchaseOrderModel.Model>("PurchaseOrder.odt",
                            singleton.GetResourceBytes("Templates.PurchaseOrder.odt"));
                }

                if (!@this.ExistPurchaseInvoiceTemplate)
                {
                    @this.PurchaseInvoiceTemplate =
                        singleton.CreateOpenDocumentTemplate<Print.PurchaseInvoiceModel.Model>("PurchaseInvoice.odt",
                            singleton.GetResourceBytes("Templates.PurchaseInvoice.odt"));
                }

                if (!@this.ExistSalesInvoiceTemplate)
                {
                    @this.SalesInvoiceTemplate =
                        singleton.CreateOpenDocumentTemplate<Print.SalesInvoiceModel.Model>("SalesInvoice.odt",
                            singleton.GetResourceBytes("Templates.SalesInvoice.odt"));
                }

                if (!@this.ExistWorkTaskTemplate)
                {
                    @this.WorkTaskTemplate =
                        singleton.CreateOpenDocumentTemplate<Print.WorkTaskModel.Model>("WorkTask.odt",
                            singleton.GetResourceBytes("Templates.WorkTask.odt"));
                }

                if (!@this.ExistWorkTaskWorkerTemplate)
                {
                    @this.WorkTaskWorkerTemplate =
                        singleton.CreateOpenDocumentTemplate<Print.WorkTaskModel.Model>("WorkTaskWorker.odt",
                            singleton.GetResourceBytes("Templates.WorkTaskWorker.odt"));
                }

                if (!@this.ExistInvoiceSequence)
                {
                    @this.InvoiceSequence = new InvoiceSequences(@this.Strategy.Transaction).EnforcedSequence;
                }

                if (!@this.ExistRequestSequence)
                {
                    @this.RequestSequence = new RequestSequences(@this.Strategy.Transaction).EnforcedSequence;
                }

                if (!@this.ExistQuoteSequence)
                {
                    @this.QuoteSequence = new QuoteSequences(@this.Strategy.Transaction).EnforcedSequence;
                }

                if (!@this.ExistCustomerShipmentSequence)
                {
                    @this.CustomerShipmentSequence = new CustomerShipmentSequences(@this.Strategy.Transaction).EnforcedSequence;
                }

                if (!@this.ExistPurchaseShipmentSequence)
                {
                    @this.PurchaseShipmentSequence = new PurchaseShipmentSequences(@this.Strategy.Transaction).EnforcedSequence;
                }

                if (!@this.ExistWorkEffortSequence)
                {
                    @this.WorkEffortSequence = new WorkEffortSequences(@this.Strategy.Transaction).EnforcedSequence;
                }

                if (!@this.ExistRequirementSequence)
                {
                    @this.RequirementSequence = new RequirementSequences(@this.Strategy.Transaction).EnforcedSequence;
                }

                if (!@this.ExistDefaultCollectionMethod && @this.Strategy.Transaction.Extent<PaymentMethod>().Count == 1)
                {
                    @this.DefaultCollectionMethod = @this.Strategy.Transaction.Extent<PaymentMethod>().First;
                }
            }
        }
        public static void AppsStartNewFiscalYear(this InternalOrganisation @this, InternalOrganisationStartNewFiscalYear method)
        {
            var organisation = (Organisation)@this;
            if (organisation.IsInternalOrganisation)
            {
                if (@this.SettingsForAccounting.ExistActualAccountingPeriod && @this.SettingsForAccounting.ActualAccountingPeriod.Active)
                {
                    return;
                }

                var year = @this.Strategy.Transaction.Now().Year;
                if (@this.SettingsForAccounting.ExistActualAccountingPeriod)
                {
                    year = @this.SettingsForAccounting.ActualAccountingPeriod.FromDate.Date.Year + 1;
                }

                var fromDate = DateTimeFactory
                    .CreateDate(year, @this.SettingsForAccounting.FiscalYearStartMonth.Value, @this.SettingsForAccounting.FiscalYearStartDay.Value).Date;

                var yearPeriod = new AccountingPeriodBuilder(@this.Strategy.Transaction)
                    .WithPeriodNumber(1)
                    .WithFrequency(new TimeFrequencies(@this.Strategy.Transaction).Year)
                    .WithFromDate(fromDate)
                    .WithThroughDate(fromDate.AddYears(1).AddSeconds(-1).Date)
                    .Build();

                var semesterPeriod = new AccountingPeriodBuilder(@this.Strategy.Transaction)
                    .WithPeriodNumber(1)
                    .WithFrequency(new TimeFrequencies(@this.Strategy.Transaction).Semester)
                    .WithFromDate(fromDate)
                    .WithThroughDate(fromDate.AddMonths(6).AddSeconds(-1).Date)
                    .WithParent(yearPeriod)
                    .Build();

                var trimesterPeriod = new AccountingPeriodBuilder(@this.Strategy.Transaction)
                    .WithPeriodNumber(1)
                    .WithFrequency(new TimeFrequencies(@this.Strategy.Transaction).Trimester)
                    .WithFromDate(fromDate)
                    .WithThroughDate(fromDate.AddMonths(3).AddSeconds(-1).Date)
                    .WithParent(semesterPeriod)
                    .Build();

                @this.SettingsForAccounting.ActualAccountingPeriod = new AccountingPeriodBuilder(@this.Strategy.Transaction)
                    .WithPeriodNumber(1)
                    .WithFrequency(new TimeFrequencies(@this.Strategy.Transaction).Month)
                    .WithFromDate(fromDate)
                    .WithThroughDate(fromDate.AddMonths(1).AddSeconds(-1).Date)
                    .WithParent(trimesterPeriod)
                    .Build();
            }

            method.StopPropagation = true;
        }


        public static int NextSubAccountNumber(this InternalOrganisation @this)
        {
            return @this.SettingsForAccounting.SubAccountCounter.NextElfProefValue();
        }

        public static string NextPurchaseInvoiceNumber(this InternalOrganisation @this, int year)
        {
            if (@this.InvoiceSequence.Equals(new InvoiceSequences(@this.Transaction()).EnforcedSequence))
            {
                return string.Concat(@this.PurchaseInvoiceNumberPrefix, @this.PurchaseInvoiceNumberCounter?.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearInternalOrganisationSequenceNumbers = @this.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);

                if (fiscalYearInternalOrganisationSequenceNumbers == null)
                {
                    fiscalYearInternalOrganisationSequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(@this.Transaction()).WithFiscalYear(year).Build();
                    @this.AddFiscalYearsInternalOrganisationSequenceNumber(fiscalYearInternalOrganisationSequenceNumbers);
                }

                return fiscalYearInternalOrganisationSequenceNumbers.NextPurchaseInvoiceNumber(year);
            }
        }

        public static string NextProductQuoteNumber(this InternalOrganisation @this, int year)
        {
            if (@this.QuoteSequence.Equals(new QuoteSequences(@this.Transaction()).EnforcedSequence))
            {
                return string.Concat(@this.ProductQuoteNumberPrefix, @this.ProductQuoteNumberCounter?.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearInternalOrganisationSequenceNumbers = @this.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);

                if (fiscalYearInternalOrganisationSequenceNumbers == null)
                {
                    fiscalYearInternalOrganisationSequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(@this.Transaction()).WithFiscalYear(year).Build();
                    @this.AddFiscalYearsInternalOrganisationSequenceNumber(fiscalYearInternalOrganisationSequenceNumbers);
                }

                return fiscalYearInternalOrganisationSequenceNumbers.NextProductQuoteNumber(year);
            }
        }

        public static string NextStatementOfWorkNumber(this InternalOrganisation @this, int year)
        {
            if (@this.QuoteSequence.Equals(new QuoteSequences(@this.Transaction()).EnforcedSequence))
            {
                return string.Concat(@this.StatementOfWorkNumberPrefix, @this.StatementOfWorkNumberCounter?.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearInternalOrganisationSequenceNumbers = @this.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);

                if (fiscalYearInternalOrganisationSequenceNumbers == null)
                {
                    fiscalYearInternalOrganisationSequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(@this.Transaction()).WithFiscalYear(year).Build();
                    @this.AddFiscalYearsInternalOrganisationSequenceNumber(fiscalYearInternalOrganisationSequenceNumbers);
                }

                return fiscalYearInternalOrganisationSequenceNumbers.NextStatementOfWorkNumber(year);
            }
        }

        public static string NextRequestNumber(this InternalOrganisation @this, int year)
        {
            if (@this.RequestSequence.Equals(new RequestSequences(@this.Transaction()).EnforcedSequence))
            {
                return string.Concat(@this.RequestNumberPrefix, @this.RequestNumberCounter?.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearInternalOrganisationSequenceNumbers = @this.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);

                if (fiscalYearInternalOrganisationSequenceNumbers == null)
                {
                    fiscalYearInternalOrganisationSequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(@this.Transaction()).WithFiscalYear(year).Build();
                    @this.AddFiscalYearsInternalOrganisationSequenceNumber(fiscalYearInternalOrganisationSequenceNumbers);
                }

                return fiscalYearInternalOrganisationSequenceNumbers.NextRequestNumber(year);
            }
        }

        public static string NextPurchaseShipmentNumber(this InternalOrganisation @this, int year)
        {
            if (@this.PurchaseShipmentSequence.Equals(new PurchaseShipmentSequences(@this.Transaction()).EnforcedSequence))
            {
                return string.Concat(@this.PurchaseShipmentNumberPrefix, @this.PurchaseShipmentNumberCounter?.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearInternalOrganisationSequenceNumbers = @this.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);

                if (fiscalYearInternalOrganisationSequenceNumbers == null)
                {
                    fiscalYearInternalOrganisationSequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(@this.Transaction()).WithFiscalYear(year).Build();
                    @this.AddFiscalYearsInternalOrganisationSequenceNumber(fiscalYearInternalOrganisationSequenceNumbers);
                }

                return fiscalYearInternalOrganisationSequenceNumbers.NextPurchaseShipmentNumber(year);
            }
        }

        public static string NextCustomerReturnNumber(this InternalOrganisation @this, int year)
        {
            if (@this.PurchaseShipmentSequence.Equals(new PurchaseShipmentSequences(@this.Transaction()).EnforcedSequence))
            {
                return string.Concat(@this.CustomerReturnNumberPrefix, @this.CustomerReturnNumberCounter?.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearInternalOrganisationSequenceNumbers = @this.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);

                if (fiscalYearInternalOrganisationSequenceNumbers == null)
                {
                    fiscalYearInternalOrganisationSequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(@this.Transaction()).WithFiscalYear(year).Build();
                    @this.AddFiscalYearsInternalOrganisationSequenceNumber(fiscalYearInternalOrganisationSequenceNumbers);
                }

                return fiscalYearInternalOrganisationSequenceNumbers.NextCustomerReturnNumber(year);
            }
        }

        public static string NextPurchaseOrderNumber(this InternalOrganisation @this, int year)
        {
            if (@this.InvoiceSequence.Equals(new InvoiceSequences(@this.Transaction()).EnforcedSequence))
            {
                return string.Concat(@this.PurchaseOrderNumberPrefix, @this.PurchaseOrderNumberCounter?.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearInternalOrganisationSequenceNumbers = @this.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);

                if (fiscalYearInternalOrganisationSequenceNumbers == null)
                {
                    fiscalYearInternalOrganisationSequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(@this.Transaction()).WithFiscalYear(year).Build();
                    @this.AddFiscalYearsInternalOrganisationSequenceNumber(fiscalYearInternalOrganisationSequenceNumbers);
                }

                return fiscalYearInternalOrganisationSequenceNumbers.NextPurchaseOrderNumber(year);
            }
        }

        public static string NextWorkEffortNumber(this InternalOrganisation @this, int year)
        {
            if (@this.WorkEffortSequence.Equals(new WorkEffortSequences(@this.Transaction()).EnforcedSequence))
            {
                return string.Concat(@this.WorkEffortNumberPrefix, @this.WorkEffortNumberCounter?.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearInternalOrganisationSequenceNumbers = @this.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);

                if (fiscalYearInternalOrganisationSequenceNumbers == null)
                {
                    fiscalYearInternalOrganisationSequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(@this.Transaction()).WithFiscalYear(year).Build();
                    @this.AddFiscalYearsInternalOrganisationSequenceNumber(fiscalYearInternalOrganisationSequenceNumbers);
                }

                return fiscalYearInternalOrganisationSequenceNumbers.NextWorkEffortNumber(year);
            }
        }

        public static string NextRequirementNumber(this InternalOrganisation @this, int year)
        {
            if (@this.RequirementSequence.Equals(new RequirementSequences(@this.Transaction()).EnforcedSequence))
            {
                return string.Concat(@this.RequirementNumberPrefix, @this.RequirementNumberCounter?.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearInternalOrganisationSequenceNumbers = @this.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);

                if (fiscalYearInternalOrganisationSequenceNumbers == null)
                {
                    fiscalYearInternalOrganisationSequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(@this.Transaction()).WithFiscalYear(year).Build();
                    @this.AddFiscalYearsInternalOrganisationSequenceNumber(fiscalYearInternalOrganisationSequenceNumbers);
                }

                return fiscalYearInternalOrganisationSequenceNumbers.NextRequirementNumber(year);
            }
        }

        public static string NextPurchaseReturnNumber(this InternalOrganisation @this, int year)
        {
            if (@this.PurchaseReturnSequence.Equals(new PurchaseReturnSequences(@this.Transaction()).EnforcedSequence))
            {
                return string.Concat(@this.PurchaseReturnNumberPrefix, @this.PurchaseReturnNumberCounter.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearInternalOrganisationSequenceNumbers = @this.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);

                if (fiscalYearInternalOrganisationSequenceNumbers == null)
                {
                    fiscalYearInternalOrganisationSequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(@this.Transaction()).WithFiscalYear(year).Build();
                    @this.AddFiscalYearsInternalOrganisationSequenceNumber(fiscalYearInternalOrganisationSequenceNumbers);
                }

                return fiscalYearInternalOrganisationSequenceNumbers.NextPurchaseReturnNumber(year);
            }
        }

        public static int NextValidElevenTestNumber(int previous)
        {
            var candidate = previous.ToString();
            var valid = false;

            while (!valid)
            {
                candidate = previous.ToString();
                var sum = 0;
                for (var i = candidate.Length; i > 0; i--)
                {
                    sum += int.Parse(candidate.Substring(candidate.Length - i, 1)) * i;
                }

                valid = sum % 11 == 0;

                if (!valid)
                {
                    previous++;
                }
            }

            return int.Parse(candidate);
        }

        public static bool AppsIsActiveSupplier(this InternalOrganisation @this, Organisation supplier, DateTime? date)
        {
            if (date == DateTime.MinValue || supplier == null)
            {
                return false;
            }

            var m = @this.Strategy.Transaction.Database.Services.Get<MetaPopulation>();

            var supplierRelationships = @this.SupplierRelationshipsWhereInternalOrganisation.Where(v => Equals(supplier, v.Supplier));
            return supplierRelationships.Any(relationship => relationship.FromDate.Date <= date && (!relationship.ExistThroughDate || relationship.ThroughDate >= date));
        }

        public static bool AppsIsActiveSubcontractor(this InternalOrganisation @this, Organisation subcontractor, DateTime? date)
        {
            if (date == DateTime.MinValue || subcontractor == null)
            {
                return false;
            }

            var subcontractorRelationships = @this.SubContractorRelationshipsWhereContractor.Where(v => Equals(subcontractor, v.SubContractor));
            return subcontractorRelationships.Any(relationship => relationship.FromDate.Date <= date
                                                             && (!relationship.ExistThroughDate || relationship.ThroughDate >= date));
        }
    }
}
