// <copyright file="RequestAnonymousDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Derivations.Rules;
    using Meta;

    public class RequestAnonymousRule : Rule
    {
        public RequestAnonymousRule(MetaPopulation m) : base(m, new Guid("c4e0b3b7-681d-46ba-8382-4a78a2e361e1")) =>
            this.Patterns = new Pattern[]
            {
                m.Request.RolePattern(v => v.Originator),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<Request>().Where(v => v.RequestState.IsAnonymous))
            {
                if (@this.ExistOriginator)
                {
                    @this.RequestState = new RequestStates(transaction).Submitted;

                    if (@this.ExistEmailAddress
                        && @this.Originator.PartyContactMechanismsWhereParty.Where(v => v.ContactMechanism.GetType().Name == typeof(EmailAddress).Name).FirstOrDefault(v => ((EmailAddress)v.ContactMechanism).ElectronicAddressString.Equals(@this.EmailAddress)) == null)
                    {
                        new PartyContactMechanismBuilder(transaction)
                            .WithParty(@this.Originator)
                            .WithContactMechanism(new EmailAddressBuilder(transaction).WithElectronicAddressString(@this.EmailAddress).Build())
                            .WithContactPurpose(new ContactMechanismPurposes(transaction).GeneralEmail)
                            .Build();
                    }

                    if (@this.ExistTelephoneNumber
                        && @this.Originator.PartyContactMechanismsWhereParty.Where(v => v.ContactMechanism.GetType().Name == typeof(TelecommunicationsNumber).Name).FirstOrDefault(v => ((TelecommunicationsNumber)v.ContactMechanism).ContactNumber.Equals(@this.TelephoneNumber)) == null)
                    {
                        new PartyContactMechanismBuilder(transaction)
                            .WithParty(@this.Originator)
                            .WithContactMechanism(new TelecommunicationsNumberBuilder(transaction).WithContactNumber(@this.TelephoneNumber).WithCountryCode(@this.TelephoneCountryCode).Build())
                            .WithContactPurpose(new ContactMechanismPurposes(transaction).GeneralPhoneNumber)
                            .Build();
                    }
                }
            }
        }
    }
}
