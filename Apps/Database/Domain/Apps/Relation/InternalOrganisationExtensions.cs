// <copyright file="InternalOrganisationExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

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
            var purchaseInvoiceNumber = @this.PurchaseInvoiceCounter.NextValue();
            return string.Concat(@this.ExistPurchaseInvoiceNumberPrefix ? @this.PurchaseInvoiceNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, purchaseInvoiceNumber);
        }

        public static string NextQuoteNumber(this InternalOrganisation @this, int year)
        {
            var quoteNumber = @this.QuoteCounter.NextValue();
            return string.Concat(@this.ExistQuoteNumberPrefix ? @this.QuoteNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, quoteNumber);
        }

        public static string NextRequestNumber(this InternalOrganisation @this, int year)
        {
            var requestNumber = @this.RequestCounter.NextValue();
            return string.Concat(@this.ExistRequestNumberPrefix ? @this.RequestNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, requestNumber);
        }

        public static string NextShipmentNumber(this InternalOrganisation @this, int year)
        {
            var shipmentNumber = @this.IncomingShipmentCounter.NextValue();
            return string.Concat(@this.ExistIncomingShipmentNumberPrefix ? @this.IncomingShipmentNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, shipmentNumber);
        }

        public static string NextPurchaseOrderNumber(this InternalOrganisation @this, int year)
        {
            var purchaseOrderNumber = @this.PurchaseOrderCounter.NextValue();
            return string.Concat(@this.ExistPurchaseOrderNumberPrefix ? @this.PurchaseOrderNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, purchaseOrderNumber);
        }

        public static string NextWorkEffortNumber(this InternalOrganisation @this)
            => string.Concat(@this.WorkEffortPrefix, @this.WorkEffortCounter.NextValue());

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
