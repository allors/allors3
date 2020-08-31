// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class PhoneCommunicationCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdPhoneCommunications= changeSet.Created.Select(v=>v.GetObject()).OfType<PhoneCommunication>();

                foreach(var phoneCommunication in createdPhoneCommunications)
                {
                    phoneCommunication.WorkItemDescription = $"Call to {phoneCommunication.ToParty?.PartyName} about {phoneCommunication.Subject}";
                }
            }
        }

        public static void PhoneCommunicationRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("fd7ee586-8d7a-434a-bcb0-68d0c3b48782")] = new PhoneCommunicationCreationDerivation();
        }
    }
}
