// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class RequestDerivation : DomainDerivation
    {
        public RequestDerivation(M m) : base(m, new Guid("AF5D09BF-9ACF-4C29-9445-6D24BE2F04E6")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.Request.Interface),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;

            foreach (var request in matches.Cast<Request>())
            {

                if (!request.ExistRecipient)
                {
                    var internalOrganisations = new Organisations(session).InternalOrganisations();

                    if (internalOrganisations.Count() == 1)
                    {
                        request.Recipient = internalOrganisations.Single();
                    }
                }

                if (request.ExistRecipient && !request.ExistRequestNumber)
                {
                    request.RequestNumber = request.Recipient.NextRequestNumber(session.Now().Year);
                    (request).SortableRequestNumber = request.Session().GetSingleton().SortableNumber(request.Recipient.RequestNumberPrefix, request.RequestNumber, request.RequestDate.Year.ToString());
                }

                if (request.ExistRequestState && request.RequestState.Equals(new RequestStates(session).Anonymous) && request.ExistOriginator)
                {
                    request.RequestState = new RequestStates(session).Submitted;

                    if (request.ExistEmailAddress
                        && request.Originator.PartyContactMechanisms.Where(v => v.ContactMechanism.GetType().Name == typeof(EmailAddress).Name).FirstOrDefault(v => ((EmailAddress)v.ContactMechanism).ElectronicAddressString.Equals(request.EmailAddress)) == null)
                    {
                        request.Originator.AddPartyContactMechanism(
                            new PartyContactMechanismBuilder(session)
                                .WithContactMechanism(new EmailAddressBuilder(session).WithElectronicAddressString(request.EmailAddress).Build())
                                .WithContactPurpose(new ContactMechanismPurposes(session).GeneralEmail)
                                .Build());
                    }

                    if (request.ExistTelephoneNumber
                        && request.Originator.PartyContactMechanisms.Where(v => v.ContactMechanism.GetType().Name == typeof(TelecommunicationsNumber).Name).FirstOrDefault(v => ((TelecommunicationsNumber)v.ContactMechanism).ContactNumber.Equals(request.TelephoneNumber)) == null)
                    {
                        request.Originator.AddPartyContactMechanism(
                            new PartyContactMechanismBuilder(session)
                                .WithContactMechanism(new TelecommunicationsNumberBuilder(session).WithContactNumber(request.TelephoneNumber).WithCountryCode(request.TelephoneCountryCode).Build())
                                .WithContactPurpose(new ContactMechanismPurposes(session).GeneralPhoneNumber)
                                .Build());
                    }
                }

                request.AddSecurityToken(new SecurityTokens(session).DefaultSecurityToken);
            }
        }
    }
}
