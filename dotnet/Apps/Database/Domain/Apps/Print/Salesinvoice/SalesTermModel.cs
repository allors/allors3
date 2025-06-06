// <copyright file="SalesTermModel.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.SalesInvoiceModel
{
    public class SalesTermModel
    {
        public SalesTermModel(SalesTerm salesTerm)
        {
            if (salesTerm is IncoTerm)
            {
                this.Name = "INCOTERMS";
            }

            if (salesTerm is Payment)
            {
                this.Name = "PAYMENT";
            }

            if (salesTerm is OrderTerm)
            {
                this.Name = "ORDERTERMS";
            }

            this.Value = salesTerm.TermValue;

            var other = new InvoiceTermTypes(salesTerm.Strategy.Transaction).Other;
            if (salesTerm.ExistTermType && !salesTerm.TermType.Equals(other))
            {
                this.Value = salesTerm.ExistTermValue ? salesTerm.TermType.Name + ": " + salesTerm.TermValue : salesTerm.TermType.Name;
            }
        }

        public string Name { get; }

        public string Value { get; }
    }
}
