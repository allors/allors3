// <copyright file="FiscalYearInternalOrganisationSequenceNumbers.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class FiscalYearInternalOrganisationSequenceNumbers
    {
        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistPurchaseOrderNumberCounter)
            {
                this.PurchaseOrderNumberCounter = new CounterBuilder(this.Transaction()).Build();
            }

            if (!this.ExistPurchaseInvoiceNumberCounter)
            {
                this.PurchaseInvoiceNumberCounter = new CounterBuilder(this.Transaction()).Build();
            }

            if (!this.ExistRequestNumberCounter)
            {
                this.RequestNumberCounter = new CounterBuilder(this.Transaction()).Build();
            }

            if (!this.ExistQuoteNumberCounter)
            {
                this.QuoteNumberCounter = new CounterBuilder(this.Transaction()).Build();
            }

            if (!this.ExistPurchaseShipmentNumberCounter)
            {
                this.PurchaseShipmentNumberCounter = new CounterBuilder(this.Transaction()).Build();
            }

            if (!this.ExistCustomerReturnNumberCounter)
            {
                this.CustomerReturnNumberCounter = new CounterBuilder(this.Transaction()).Build();
            }

            if (!this.ExistIncomingTransferNumberCounter)
            {
                this.IncomingTransferNumberCounter = new CounterBuilder(this.Transaction()).Build();
            }

            if (!this.ExistWorkEffortNumberCounter)
            {
                this.WorkEffortNumberCounter = new CounterBuilder(this.Transaction()).Build();
            }
        }

        public string NextPurchaseOrderNumber(int year)
        {
            var number = this.PurchaseOrderNumberCounter.NextValue();
            return string.Concat(this.ExistPurchaseOrderNumberPrefix ? this.PurchaseOrderNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, number);
        }

        public string NextPurchaseInvoiceNumber(int year)
        {
            var number = this.PurchaseInvoiceNumberCounter.NextValue();
            return string.Concat(this.ExistPurchaseInvoiceNumberPrefix ? this.PurchaseInvoiceNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, number);
        }

        public string NextRequestNumber(int year)
        {
            var number = this.RequestNumberCounter.NextValue();
            return string.Concat(this.ExistRequestNumberPrefix ? this.RequestNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, number);
        }

        public string NextQuoteNumber(int year)
        {
            var number = this.QuoteNumberCounter.NextValue();
            return string.Concat(this.ExistQuoteNumberPrefix ? this.QuoteNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, number);
        }

        public string NextPurchaseShipmentNumber(int year)
        {
            var number = this.PurchaseShipmentNumberCounter.NextValue();
            return string.Concat(this.ExistPurchaseShipmentNumberPrefix ? this.PurchaseShipmentNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, number);
        }

        public string NextCustomerReturnNumber(int year)
        {
            var number = this.CustomerReturnNumberCounter.NextValue();
            return string.Concat(this.ExistCustomerReturnNumberPrefix ? this.CustomerReturnNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, number);
        }

        public string NextIncomingTransferNumber(int year)
        {
            var number = this.IncomingTransferNumberCounter.NextValue();
            return string.Concat(this.ExistIncomingTransferNumberPrefix ? this.IncomingTransferNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, number);
        }

        public string NextWorkEffortNumber(int year)
        {
            var number = this.WorkEffortNumberCounter.NextValue();
            return string.Concat(this.ExistWorkEffortNumberPrefix ? this.WorkEffortNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, number);
        }
    }
}
