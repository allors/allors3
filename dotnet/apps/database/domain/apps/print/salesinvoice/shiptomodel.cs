// <copyright file="ShipToModel.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.SalesInvoiceModel
{
    public class ShipToModel
    {
        public ShipToModel(SalesInvoice invoice)
        {
            var shipTo = invoice.ShipToCustomer ?? invoice.BillToCustomer;
            var shipToOrganisation = shipTo as Organisation;

            if (shipTo != null)
            {
                this.Name = shipTo.PartyName;
                this.TaxId = shipToOrganisation?.TaxNumber;
            }

            this.Contact = invoice.ShipToContactPerson?.PartyName;

            var shipToAddress = invoice.DerivedShipToAddress ??
                                invoice.ShipToCustomer?.ShippingAddress ??
                                invoice.ShipToCustomer?.GeneralCorrespondence ??
                                invoice.DerivedBillToContactMechanism as PostalAddress ??
                                invoice.BillToCustomer?.ShippingAddress ??
                                invoice.BillToCustomer?.GeneralCorrespondence;

            if (shipToAddress is PostalAddress postalAddress)
            {
                this.Address = postalAddress.Address1;
                if (!string.IsNullOrWhiteSpace(postalAddress.Address2))
                {
                    this.Address = $"\n{postalAddress.Address2}";
                }

                if (!string.IsNullOrWhiteSpace(postalAddress.Address3))
                {
                    this.Address = $"\n{postalAddress.Address3}";
                }

                this.City = postalAddress.Locality;
                this.State = postalAddress.Region;
                this.PostalCode = postalAddress.PostalCode;
                this.Country = postalAddress.Country?.Name;
            }
        }

        public string Name { get; }

        public string Address { get; }

        public string City { get; }

        public string State { get; }

        public string Country { get; }

        public string PostalCode { get; }

        public string TaxId { get; }

        public string Contact { get; }
    }
}
