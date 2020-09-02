// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class FaxCommunicationCreationDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("A6D89A8A-641F-4D11-8E92-CC10A7A2A89E");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.FaxCommunication.Class),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var faxCommunication in matches.Cast<FaxCommunication>())
            {
                faxCommunication.WorkItemDescription = $"Fax to {faxCommunication.ToParty.PartyName} about {faxCommunication.Subject}";
            }
        }
    }
}
