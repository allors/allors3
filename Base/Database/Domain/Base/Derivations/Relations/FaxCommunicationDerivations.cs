// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class FaxCommunicationCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdFaxCommunications = changeSet.Created.Select(v=>v.GetObject()).OfType<FaxCommunication>();

                foreach (var faxCommunication in createdFaxCommunications)
                {
                    faxCommunication.WorkItemDescription = $"Fax to {faxCommunication.ToParty.PartyName} about {faxCommunication.Subject}";
                }
            }
        }

        public static void FaxCommunicationRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("d363711d-767e-42ae-a1f7-6cb0734ee42c")] = new FaxCommunicationCreationDerivation();
        }
    }
}
