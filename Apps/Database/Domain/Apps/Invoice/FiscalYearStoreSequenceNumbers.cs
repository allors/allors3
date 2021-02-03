// <copyright file="FiscalYearStoreSequenceNumbers.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class FiscalYearStoreSequenceNumbers
    {
        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistSalesInvoiceNumberCounter)
            {
                this.SalesInvoiceNumberCounter = new CounterBuilder(this.Session()).Build();
            }

            if (!this.ExistCreditNoteNumberCounter)
            {
                this.CreditNoteNumberCounter = new CounterBuilder(this.Session()).Build();
            }

            if (!this.ExistCustomerShipmentNumberCounter)
            {
                this.CustomerShipmentNumberCounter = new CounterBuilder(this.Session()).Build();
            }

            if (!this.ExistPurchaseReturnNumberCounter)
            {
                this.PurchaseReturnNumberCounter = new CounterBuilder(this.Session()).Build();
            }

            if (!this.ExistDropShipmentNumberCounter)
            {
                this.DropShipmentNumberCounter = new CounterBuilder(this.Session()).Build();
            }

            if (!this.ExistOutgoingTransferNumberCounter)
            {
                this.OutgoingTransferNumberCounter = new CounterBuilder(this.Session()).Build();
            }

            if (!this.ExistSalesOrderNumberCounter)
            {
                this.SalesOrderNumberCounter = new CounterBuilder(this.Session()).Build();
            }
        }

        public string NextSalesInvoiceNumber(int year)
        {
            var number = this.SalesInvoiceNumberCounter.NextValue();
            return string.Concat(this.ExistSalesInvoiceNumberPrefix ? this.SalesInvoiceNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, number);
        }

        public string NextCreditNoteNumber(int year)
        {
            var number = this.CreditNoteNumberCounter.NextValue();
            return string.Concat(this.ExistCreditNoteNumberPrefix ? this.CreditNoteNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, number);
        }

        public string NextCustomerShipmentNumber(int year)
        {
            var number = this.CustomerShipmentNumberCounter.NextValue();
            return string.Concat(this.ExistCustomerShipmentNumberPrefix ? this.CustomerShipmentNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, number);
        }

        public string NextPurchaseReturnNumber(int year)
        {
            var number = this.PurchaseReturnNumberCounter.NextValue();
            return string.Concat(this.ExistPurchaseReturnNumberPrefix ? this.PurchaseReturnNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, number);
        }

        public string NextDropShipmentNumber(int year)
        {
            var number = this.DropShipmentNumberCounter.NextValue();
            return string.Concat(this.ExistDropShipmentNumberPrefix ? this.DropShipmentNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, number);
        }

        public string NextOutgoingTransferNumber(int year)
        {
            var number = this.OutgoingTransferNumberCounter.NextValue();
            return string.Concat(this.ExistOutgoingTransferNumberPrefix ? this.OutgoingTransferNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, number);
        }

        public string NextSalesOrderNumber(int year)
        {
            var number = this.SalesOrderNumberCounter.NextValue();
            return string.Concat(this.ExistSalesOrderNumberPrefix ? this.SalesOrderNumberPrefix.Replace("{year}", year.ToString()) : string.Empty, number);
        }
    }
}
