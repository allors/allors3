// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BillToModel.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.Print.ProposalModel
{
    using System.Collections.Generic;

    public class BillToModel
    {
        public BillToModel(Proposal quote, Dictionary<string, byte[]> imageByImageName)
        {
            var contactMechanism = quote.FullfillContactMechanism;

            var address = new List<string>();

            if (contactMechanism is PostalAddress postalAddress)
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
            }

            if (contactMechanism is ElectronicAddress electronicAddress)
            {
                address.Add(electronicAddress.ElectronicAddressString);
            }

            this.Address = address.ToArray();
        }

        public string[] Address { get; }

        public string City { get; }

        public string State { get; }

        public string Country { get; }

        public string PostalCode { get; }
    }
}
