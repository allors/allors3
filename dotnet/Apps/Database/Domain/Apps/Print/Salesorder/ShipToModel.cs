// <copyright file="ShipToModel.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.SalesOrderModel
{
    using System.Collections.Generic;

    public class ShipToModel
    {
        public ShipToModel(SalesOrder order)
        {
            var shipTo = order.ShipToCustomer ?? order.BillToCustomer;
            var shipToOrganisation = shipTo as Organisation;

            if (shipTo != null)
            {
                this.Name = shipTo.DisplayName;
                this.TaxId = shipToOrganisation?.TaxNumber;
            }

            this.Contact = order.ShipToContactPerson?.DisplayName;

            var shipToAddress = order.DerivedShipToAddress ??
                                order.ShipToCustomer?.ShippingAddress ??
                                order.ShipToCustomer?.GeneralCorrespondence ??
                                order.DerivedBillToContactMechanism as PostalAddress ??
                                order.BillToCustomer?.ShippingAddress ??
                                order.BillToCustomer?.GeneralCorrespondence;

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
