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
        public class LetterCorrespondenceCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdLetterCorrespondence = changeSet.Created.Select(session.Instantiate).OfType<LetterCorrespondence>();

                foreach (var letterCorrespondence in createdLetterCorrespondence)
                {
                    letterCorrespondence.WorkItemDescription = $"Letter to {letterCorrespondence.ToParty.PartyName} about {letterCorrespondence.Subject}";
                }
            }
        }

        public static void LetterCorrespondenceRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("fbcdb4b9-8786-4be6-9c83-ab84a2a88981")] = new LetterCorrespondenceCreationDerivation();
        }
    }
}
