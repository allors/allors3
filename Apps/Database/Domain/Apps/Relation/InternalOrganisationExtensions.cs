// <copyright file="InternalOrganisationExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Allors.Database.Meta;

namespace Allors.Database.Domain
{
    public static partial class InternalOrganisationExtensions
    {
        public static void AppsOnInit(this InternalOrganisation @this, ObjectOnInit method)
        {
            var singleton = @this.Session().GetSingleton();

            if (@this.IsInternalOrganisation)
            {
                if (!@this.ExistProductQuoteTemplate)
                {
                    @this.ProductQuoteTemplate =
                        singleton.CreateOpenDocumentTemplate<Print.ProductQuoteModel.Model>("ProductQuote.odt",
                            singleton.GetResourceBytes("Templates.ProductQuote.odt"));
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
            }
        }

        public static void AppsStartNewFiscalYear(this InternalOrganisation @this, InternalOrganisationStartNewFiscalYear method)
        {
            var organisation = (Organisation)@this;
            if (organisation.IsInternalOrganisation)
            {
                if (@this.ExistActualAccountingPeriod && @this.ActualAccountingPeriod.Active)
                {
                    return;
                }

                var year = @this.Strategy.Session.Now().Year;
                if (@this.ExistActualAccountingPeriod)
                {
                    year = @this.ActualAccountingPeriod.FromDate.Date.Year + 1;
                }

                var fromDate = DateTimeFactory
                    .CreateDate(year, @this.FiscalYearStartMonth.Value, @this.FiscalYearStartDay.Value).Date;

                var yearPeriod = new AccountingPeriodBuilder(@this.Strategy.Session)
                    .WithPeriodNumber(1)
                    .WithFrequency(new TimeFrequencies(@this.Strategy.Session).Year)
                    .WithFromDate(fromDate)
                    .WithThroughDate(fromDate.AddYears(1).AddSeconds(-1).Date)
                    .Build();

                var semesterPeriod = new AccountingPeriodBuilder(@this.Strategy.Session)
                    .WithPeriodNumber(1)
                    .WithFrequency(new TimeFrequencies(@this.Strategy.Session).Semester)
                    .WithFromDate(fromDate)
                    .WithThroughDate(fromDate.AddMonths(6).AddSeconds(-1).Date)
                    .WithParent(yearPeriod)
                    .Build();

                var trimesterPeriod = new AccountingPeriodBuilder(@this.Strategy.Session)
                    .WithPeriodNumber(1)
                    .WithFrequency(new TimeFrequencies(@this.Strategy.Session).Trimester)
                    .WithFromDate(fromDate)
                    .WithThroughDate(fromDate.AddMonths(3).AddSeconds(-1).Date)
                    .WithParent(semesterPeriod)
                    .Build();

                var monthPeriod = new AccountingPeriodBuilder(@this.Strategy.Session)
                    .WithPeriodNumber(1)
                    .WithFrequency(new TimeFrequencies(@this.Strategy.Session).Month)
                    .WithFromDate(fromDate)
                    .WithThroughDate(fromDate.AddMonths(1).AddSeconds(-1).Date)
                    .WithParent(trimesterPeriod)
                    .Build();

                @this.ActualAccountingPeriod = monthPeriod;
            }
        }

        public static int NextSubAccountNumber(this InternalOrganisation @this)
        {
            var next = @this.SubAccountCounter.NextElfProefValue();
            return next;
        }

        public static string NextPurchaseInvoiceNumber(this InternalOrganisation @this, int year)
        {
            var m = @this.Session().Database.Context().M;

            if (@this.InvoiceSequence.Equals(new InvoiceSequences(@this.Session()).EnforcedSequence))
            {
                return string.Concat(@this.PurchaseInvoiceNumberPrefix, @this.PurchaseInvoiceNumberCounter.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearsInternalOrganisationSequenceNumbers = new FiscalYearsInternalOrganisationSequenceNumbers(@this.Session()).Extent();
                fiscalYearsInternalOrganisationSequenceNumbers.Filter.AddEquals(m.FiscalYearInternalOrganisationSequenceNumbers.FiscalYear, year);
                var fiscalYearInternalOrganisationSequenceNumbers = fiscalYearsInternalOrganisationSequenceNumbers.First;

                if (fiscalYearInternalOrganisationSequenceNumbers == null)
                {
                    fiscalYearInternalOrganisationSequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(@this.Session()).WithFiscalYear(year).Build();
                    @this.AddFiscalYearsInternalOrganisationSequenceNumber(fiscalYearInternalOrganisationSequenceNumbers);
                }

                return fiscalYearInternalOrganisationSequenceNumbers.NextPurchaseInvoiceNumber(year);
            }
        }

        public static string NextQuoteNumber(this InternalOrganisation @this, int year)
        {
            var m = @this.Session().Database.Context().M;

            if (@this.InvoiceSequence.Equals(new InvoiceSequences(@this.Session()).EnforcedSequence))
            {
                return string.Concat(@this.QuoteNumberPrefix, @this.QuoteNumberCounter.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearsInternalOrganisationSequenceNumbers = new FiscalYearsInternalOrganisationSequenceNumbers(@this.Session()).Extent();
                fiscalYearsInternalOrganisationSequenceNumbers.Filter.AddEquals(m.FiscalYearInternalOrganisationSequenceNumbers.FiscalYear, year);
                var fiscalYearInternalOrganisationSequenceNumbers = fiscalYearsInternalOrganisationSequenceNumbers.First;

                if (fiscalYearInternalOrganisationSequenceNumbers == null)
                {
                    fiscalYearInternalOrganisationSequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(@this.Session()).WithFiscalYear(year).Build();
                    @this.AddFiscalYearsInternalOrganisationSequenceNumber(fiscalYearInternalOrganisationSequenceNumbers);
                }

                return fiscalYearInternalOrganisationSequenceNumbers.NextQuoteNumber(year);
            }
        }

        public static string NextRequestNumber(this InternalOrganisation @this, int year)
        {
            var m = @this.Session().Database.Context().M;

            if (@this.InvoiceSequence.Equals(new InvoiceSequences(@this.Session()).EnforcedSequence))
            {
                return string.Concat(@this.RequestNumberPrefix, @this.RequestNumberCounter.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearsInternalOrganisationSequenceNumbers = new FiscalYearsInternalOrganisationSequenceNumbers(@this.Session()).Extent();
                fiscalYearsInternalOrganisationSequenceNumbers.Filter.AddEquals(m.FiscalYearInternalOrganisationSequenceNumbers.FiscalYear, year);
                var fiscalYearInternalOrganisationSequenceNumbers = fiscalYearsInternalOrganisationSequenceNumbers.First;

                if (fiscalYearInternalOrganisationSequenceNumbers == null)
                {
                    fiscalYearInternalOrganisationSequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(@this.Session()).WithFiscalYear(year).Build();
                    @this.AddFiscalYearsInternalOrganisationSequenceNumber(fiscalYearInternalOrganisationSequenceNumbers);
                }

                return fiscalYearInternalOrganisationSequenceNumbers.NextRequestNumber(year);
            }
        }

        public static string NextShipmentNumber(this InternalOrganisation @this, int year)
        {
            var m = @this.Session().Database.Context().M;

            if (@this.InvoiceSequence.Equals(new InvoiceSequences(@this.Session()).EnforcedSequence))
            {
                return string.Concat(@this.IncomingShipmentNumberPrefix, @this.IncomingShipmentNumberCounter.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearsInternalOrganisationSequenceNumbers = new FiscalYearsInternalOrganisationSequenceNumbers(@this.Session()).Extent();
                fiscalYearsInternalOrganisationSequenceNumbers.Filter.AddEquals(m.FiscalYearInternalOrganisationSequenceNumbers.FiscalYear, year);
                var fiscalYearInternalOrganisationSequenceNumbers = fiscalYearsInternalOrganisationSequenceNumbers.First;

                if (fiscalYearInternalOrganisationSequenceNumbers == null)
                {
                    fiscalYearInternalOrganisationSequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(@this.Session()).WithFiscalYear(year).Build();
                    @this.AddFiscalYearsInternalOrganisationSequenceNumber(fiscalYearInternalOrganisationSequenceNumbers);
                }

                return fiscalYearInternalOrganisationSequenceNumbers.NextIncomingShipmentNumber(year);
            }
        }

        public static string NextPurchaseOrderNumber(this InternalOrganisation @this, int year)
        {
            var m = @this.Session().Database.Context().M;

            if (@this.InvoiceSequence.Equals(new InvoiceSequences(@this.Session()).EnforcedSequence))
            {
                return string.Concat(@this.PurchaseOrderNumberPrefix, @this.PurchaseOrderNumberCounter.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearsInternalOrganisationSequenceNumbers = new FiscalYearsInternalOrganisationSequenceNumbers(@this.Session()).Extent();
                fiscalYearsInternalOrganisationSequenceNumbers.Filter.AddEquals(m.FiscalYearInternalOrganisationSequenceNumbers.FiscalYear, year);
                var fiscalYearInternalOrganisationSequenceNumbers = fiscalYearsInternalOrganisationSequenceNumbers.First;

                if (fiscalYearInternalOrganisationSequenceNumbers == null)
                {
                    fiscalYearInternalOrganisationSequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(@this.Session()).WithFiscalYear(year).Build();
                    @this.AddFiscalYearsInternalOrganisationSequenceNumber(fiscalYearInternalOrganisationSequenceNumbers);
                }

                return fiscalYearInternalOrganisationSequenceNumbers.NextPurchaseOrderNumber(year);
            }
        }

        public static string NextWorkEffortNumber(this InternalOrganisation @this, int year)
        {
            var m = @this.Session().Database.Context().M;

            if (@this.InvoiceSequence.Equals(new InvoiceSequences(@this.Session()).EnforcedSequence))
            {
                return string.Concat(@this.WorkEffortNumberPrefix, @this.WorkEffortNumberCounter.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearsInternalOrganisationSequenceNumbers = new FiscalYearsInternalOrganisationSequenceNumbers(@this.Session()).Extent();
                fiscalYearsInternalOrganisationSequenceNumbers.Filter.AddEquals(m.FiscalYearInternalOrganisationSequenceNumbers.FiscalYear, year);
                var fiscalYearInternalOrganisationSequenceNumbers = fiscalYearsInternalOrganisationSequenceNumbers.First;

                if (fiscalYearInternalOrganisationSequenceNumbers == null)
                {
                    fiscalYearInternalOrganisationSequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(@this.Session()).WithFiscalYear(year).Build();
                    @this.AddFiscalYearsInternalOrganisationSequenceNumber(fiscalYearInternalOrganisationSequenceNumbers);
                }

                return fiscalYearInternalOrganisationSequenceNumbers.NextWorkEffortNumber(year);
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
    }
}
