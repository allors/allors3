// <copyright file="ShipToModel.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.SalesInvoiceModel
{
    using System.Collections.Generic;

    public class ShipToModel
    {
        public ShipToModel(SalesInvoice invoice)
        {
            var shipTo = invoice.ShipToCustomer ?? invoice.BillToCustomer;
            var shipToOrganisation = shipTo as Organisation;

            if (shipTo != null)
            {
                this.Name = shipTo.DisplayName;
                this.TaxId = shipToOrganisation?.TaxNumber;
            }

            this.Contact = invoice.ShipToContactPerson?.DisplayName;

            var shipToAddress = invoice.DerivedShipToAddress ??
                                invoice.ShipToCustomer?.ShippingAddress ??
                                invoice.ShipToCustomer?.GeneralCorrespondence ??
                                invoice.DerivedBillToContactMechanism as PostalAddress ??
                                invoice.BillToCustomer?.ShippingAddress ??
                                invoice.BillToCustomer?.GeneralCorrespondence;

            var address = new List<string>();

            if (shipToAddress is PostalAddress postalAddress)
            {
                address.Add(postalAddress.Address1);
                if (!string.IsNullOrWhiteSpace(postalAddress.Address2))
                {
                    address.Add(postalAddress.Address2);
                }

                if (!string.IsNullOrWhiteSpace(postalAddress.Address3))
                {
                    address.Add(postalAddress.Address3);
                }

                this.City = postalAddress.Locality;
                this.State = postalAddress.Region;
                this.PostalCode = postalAddress.PostalCode;
                this.Country = postalAddress.Country?.Name;
                this.PrintPostalCode = !string.IsNullOrEmpty(this.PostalCode);
                this.PrintCity = !this.PrintPostalCode;
            }

            this.Address = address.ToArray();
        }

        public bool PrintPostalCode { get; }

        public bool PrintCity { get; }

        public string Name { get; }

        public string[] Address { get; }

        public string City { get; }

        public string State { get; }

        public string Country { get; }

        public string PostalCode { get; }

        public string TaxId { get; }

        public string Contact { get; }
    }
}
