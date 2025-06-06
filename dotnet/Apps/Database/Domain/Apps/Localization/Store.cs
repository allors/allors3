// <copyright file="Store.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;

    public partial class Store
    {
        public string NextSalesInvoiceNumber(int year)
        {
            if (this.InternalOrganisation.InvoiceSequence.Equals(new InvoiceSequences(this.Strategy.Transaction).EnforcedSequence))
            {
                return string.Concat(this.SalesInvoiceNumberPrefix, this.SalesInvoiceNumberCounter.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearStoreSequenceNumbers = this.FiscalYearsStoreSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);

                if (fiscalYearStoreSequenceNumbers == null)
                {
                    fiscalYearStoreSequenceNumbers = new FiscalYearStoreSequenceNumbersBuilder(this.Strategy.Transaction).WithFiscalYear(year).Build();
                    this.AddFiscalYearsStoreSequenceNumber(fiscalYearStoreSequenceNumbers);
                }

                return fiscalYearStoreSequenceNumbers.NextSalesInvoiceNumber(year);
            }
        }

        public string LastSalesInvoiceNumber(int year)
        {
            if (this.InternalOrganisation.InvoiceSequence.Equals(new InvoiceSequences(this.Strategy.Transaction).EnforcedSequence))
            {
                return string.Concat(this.SalesInvoiceNumberPrefix, this.SalesInvoiceNumberCounter.Value).Replace("{year}", year.ToString());
            }

            var fiscalYearStoreSequenceNumbers = this.FiscalYearsStoreSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);

            if (fiscalYearStoreSequenceNumbers != null)
            {
                return string.Concat(fiscalYearStoreSequenceNumbers.ExistSalesInvoiceNumberPrefix ?
                    fiscalYearStoreSequenceNumbers.SalesInvoiceNumberPrefix.Replace("{year}", year.ToString())
                    : string.Empty,
                    fiscalYearStoreSequenceNumbers.SalesInvoiceNumberCounter.Value);
            }

            return null;
        }

        public string NextCreditNoteNumber(int year)
        {
            if (this.InternalOrganisation.InvoiceSequence.Equals(new InvoiceSequences(this.Strategy.Transaction).EnforcedSequence))
            {
                return string.Concat(this.CreditNoteNumberPrefix, this.CreditNoteNumberCounter.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearStoreSequenceNumbers = this.FiscalYearsStoreSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);

                if (fiscalYearStoreSequenceNumbers == null)
                {
                    fiscalYearStoreSequenceNumbers = new FiscalYearStoreSequenceNumbersBuilder(this.Strategy.Transaction).WithFiscalYear(year).Build();
                    this.AddFiscalYearsStoreSequenceNumber(fiscalYearStoreSequenceNumbers);
                }

                return fiscalYearStoreSequenceNumbers.NextCreditNoteNumber(year);
            }
        }

        public string NextCustomerShipmentNumber(int year)
        {
            if (this.InternalOrganisation.CustomerShipmentSequence.Equals(new CustomerShipmentSequences(this.Strategy.Transaction).EnforcedSequence))
            {
                return string.Concat(this.CustomerShipmentNumberPrefix, this.CustomerShipmentNumberCounter.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearStoreSequenceNumbers = this.FiscalYearsStoreSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);

                if (fiscalYearStoreSequenceNumbers == null)
                {
                    fiscalYearStoreSequenceNumbers = new FiscalYearStoreSequenceNumbersBuilder(this.Strategy.Transaction).WithFiscalYear(year).Build();
                    this.AddFiscalYearsStoreSequenceNumber(fiscalYearStoreSequenceNumbers);
                }

                return fiscalYearStoreSequenceNumbers.NextCustomerShipmentNumber(year);
            }
        }

        public string NextDropShipmentNumber(int year)
        {
            if (this.InternalOrganisation.DropShipmentSequence.Equals(new DropShipmentSequences(this.Strategy.Transaction).EnforcedSequence))
            {
                return string.Concat(this.DropShipmentNumberPrefix, this.DropShipmentNumberCounter.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearStoreSequenceNumbers = this.FiscalYearsStoreSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);

                if (fiscalYearStoreSequenceNumbers == null)
                {
                    fiscalYearStoreSequenceNumbers = new FiscalYearStoreSequenceNumbersBuilder(this.Strategy.Transaction).WithFiscalYear(year).Build();
                    this.AddFiscalYearsStoreSequenceNumber(fiscalYearStoreSequenceNumbers);
                }

                return fiscalYearStoreSequenceNumbers.NextDropShipmentNumber(year);
            }
        }

        public string NextSalesOrderNumber(int year)
        {
            if (this.InternalOrganisation.InvoiceSequence.Equals(new InvoiceSequences(this.Strategy.Transaction).EnforcedSequence))
            {
                return string.Concat(this.SalesOrderNumberPrefix, this.SalesOrderNumberCounter.NextValue()).Replace("{year}", year.ToString());
            }
            else
            {
                var fiscalYearStoreSequenceNumbers = this.FiscalYearsStoreSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);

                if (fiscalYearStoreSequenceNumbers == null)
                {
                    fiscalYearStoreSequenceNumbers = new FiscalYearStoreSequenceNumbersBuilder(this.Strategy.Transaction).WithFiscalYear(year).Build();
                    this.AddFiscalYearsStoreSequenceNumber(fiscalYearStoreSequenceNumbers);
                }

                return fiscalYearStoreSequenceNumbers.NextSalesOrderNumber(year);
            }
        }

        public string NextTemporaryInvoiceNumber() => this.SalesInvoiceTemporaryCounter.NextValue().ToString();

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistAutoGenerateCustomerShipment)
            {
                this.AutoGenerateCustomerShipment = true;
            }

            if (!this.ExistAutoGenerateShipmentPackage)
            {
                this.AutoGenerateShipmentPackage = true;
            }

            if (!this.ExistBillingProcess)
            {
                this.BillingProcess = new BillingProcesses(this.Strategy.Transaction).BillingForShipmentItems;
            }

            if (!this.ExistSalesInvoiceTemporaryCounter)
            {
                this.SalesInvoiceTemporaryCounter = new CounterBuilder(this.Strategy.Transaction).WithUniqueId(Guid.NewGuid()).WithValue(0).Build();
            }

            var internalOrganisations = new Organisations(this.Strategy.Transaction).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

            if (!this.ExistInternalOrganisation && internalOrganisations.Length == 1)
            {
                this.InternalOrganisation = internalOrganisations.First();
            }

            if (!this.ExistDefaultFacility)
            {
                this.DefaultFacility = this.Strategy.Transaction.GetSingleton().Settings.DefaultFacility;
            }
        }
    }
}
