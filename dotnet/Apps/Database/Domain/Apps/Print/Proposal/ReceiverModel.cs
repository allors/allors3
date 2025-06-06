// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReceiverModel.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.Print.ProposalModel
{
    using System.Collections.Generic;
    using System.Linq;

    public class ReceiverModel
    {
        public ReceiverModel(Proposal quote, Dictionary<string, byte[]> imageByImageName)
        {
            var transaction = quote.Strategy.Transaction;

            var receiver = quote.Receiver;
            var organisationReceiver = quote.Receiver as Organisation;

            if (receiver != null)
            {
                this.Name = receiver.DisplayName;
                this.TaxId = organisationReceiver?.TaxNumber;
            }

            this.Contact = quote.ContactPerson?.DisplayName;
            this.ContactFirstName = quote.ContactPerson?.FirstName;
            this.Salutation = quote.ContactPerson?.Salutation?.Name;
            this.ContactFunction = quote.ContactPerson?.Function;

            if (quote.ContactPerson?.CurrentPartyContactMechanisms.FirstOrDefault(v => v.ContactMechanism.GetType().Name == typeof(EmailAddress).Name)?.ContactMechanism is EmailAddress emailAddress)
            {
                this.ContactEmail = emailAddress.ElectronicAddressString;
            }

            if (quote.ContactPerson?.CurrentPartyContactMechanisms.FirstOrDefault(v => v.ContactMechanism.GetType().Name == typeof(TelecommunicationsNumber).Name)?.ContactMechanism is TelecommunicationsNumber phone)
            {
                this.ContactPhone = phone.ToString();
            }
        }

        public string Name { get; }

        public string Salutation { get; }

        public string Contact { get; }

        public string ContactFirstName { get; }

        public string ContactFunction { get; }

        public string ContactEmail { get; }

        public string ContactPhone { get; }

        public string TaxId { get; }
    }
}
