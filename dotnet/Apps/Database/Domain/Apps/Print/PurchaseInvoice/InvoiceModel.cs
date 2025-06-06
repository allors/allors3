// <copyright file="InvoiceModel.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.PurchaseInvoiceModel
{
    using System.Globalization;
    using System.Linq;

    public class InvoiceModel
    {
        public InvoiceModel(PurchaseInvoice invoice)
        {
            this.Description = invoice.Description?.Split('\n');
            this.Number = invoice.InvoiceNumber;
            this.Date = invoice.InvoiceDate.ToString("yyyy-MM-dd");
            this.CustomerReference = invoice.CustomerReference;

            this.SubTotal = invoice.TotalBasePrice.ToString("N2", new CultureInfo("nl-BE"));
            this.TotalExVat = invoice.TotalExVat.ToString("N2", new CultureInfo("nl-BE"));
            this.VatRate = invoice.DerivedVatRate?.Rate.ToString("n2")
                ?? invoice.ValidInvoiceItems.FirstOrDefault(v => v.ExistVatRate)?.VatRate.Rate.ToString("n2")
                ?? "0";
            this.TotalVat = invoice.TotalVat.ToString("N2", new CultureInfo("nl-BE"));
            this.IrpfRate = invoice.DerivedIrpfRate?.Rate.ToString("n2");

            // IRPF is subtracted for total amount to pay
            var totalIrpf = invoice.TotalIrpf * -1;
            this.TotalIrpf = totalIrpf.ToString("N2", new CultureInfo("nl-BE"));
            this.PrintIrpf = invoice.TotalIrpf != 0;

            this.TotalIncVat = invoice.TotalIncVat.ToString("N2", new CultureInfo("nl-BE"));

            var currencyIsoCode = invoice.DerivedCurrency.IsoCode;
            this.GrandTotal = currencyIsoCode + " " + invoice.GrandTotal.ToString("N2", new CultureInfo("nl-BE"));
        }

        public string[] Description { get; }

        public string Number { get; }

        public string Date { get; }

        public string CustomerReference { get; }

        public string SubTotal { get; }

        public string TotalExVat { get; }

        public string VatRate { get; }

        public string TotalVat { get; }

        public string IrpfRate { get; }

        public string TotalIrpf { get; }

        public string TotalIncVat { get; }

        public string GrandTotal { get; }

        public bool PrintIrpf { get; }
    }
}
