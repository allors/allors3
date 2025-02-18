// <copyright file="BilledToModel.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.PurchaseInvoiceModel
{
    using System.Collections.Generic;
    using System.Linq;

    public class BilledToModel
    {
        public BilledToModel(Organisation orderedBy)
        {
            if (orderedBy != null)
            {
                this.Name = orderedBy.DisplayName;
                this.Email = orderedBy.GeneralEmail?.ElectronicAddressString;
                this.Website = orderedBy.InternetAddress?.ElectronicAddressString;
                this.TaxId = orderedBy.TaxNumber;

                var phone = orderedBy.BillingInquiriesPhone ?? orderedBy.GeneralPhoneNumber;
                if (phone != null)
                {
                    this.Telephone = $"{phone.CountryCode} {phone.AreaCode} {phone.ContactNumber}";
                }

                var address = new List<string>();

                if (orderedBy.GeneralCorrespondence is PostalAddress generalAddress)
                {
                    address.Add(generalAddress.Address1);
                    if (!string.IsNullOrWhiteSpace(generalAddress.Address2))
                    {
                        address.Add(generalAddress.Address2);
                    }

                    if (!string.IsNullOrWhiteSpace(generalAddress.Address3))
                    {
                        address.Add(generalAddress.Address3);
                    }

                    this.City = generalAddress.Locality;
                    this.State = generalAddress.Region;
                    this.PostalCode = generalAddress.PostalCode;
                    this.Country = generalAddress.Country?.Name;
                    this.PrintPostalCode = !string.IsNullOrEmpty(this.PostalCode);
                    this.PrintCity = !this.PrintPostalCode;
                }

                var bankAccount = orderedBy.BankAccounts.FirstOrDefault(v => v.ExistIban);
                if (bankAccount != null)
                {
                    this.IBAN = bankAccount.Iban;

                    var bank = bankAccount.Bank;
                    this.Bank = bank?.Name;
                    this.Swift = bank?.SwiftCode ?? bank?.Bic;
                }

                this.Address = address.ToArray();
            }
        }

        public bool PrintPostalCode { get; }

        public bool PrintCity { get; }

        public string Name { get; }

        public string[] Address { get; }

        public string City { get; }

        public string State { get; }

        public string Country { get; }

        public string PostalCode { get; }

        public string Telephone { get; }

        public string Email { get; }

        public string Website { get; }

        public string Bank { get; }

        public string BankAccount { get; }

        public string IBAN { get; }

        public string Swift { get; }

        public string TaxId { get; }
    }
}
