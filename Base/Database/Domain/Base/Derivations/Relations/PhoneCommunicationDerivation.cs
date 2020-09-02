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

    public class PhoneCommunicationDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("6C8EB5C8-1E8C-41CF-B127-53C88E200F65");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.PhoneCommunication.Class),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var phoneCommunication in matches.Cast<PhoneCommunication>())
            {
                phoneCommunication.WorkItemDescription = $"Call to {phoneCommunication.ToParty?.PartyName} about {phoneCommunication.Subject}";
            }
        }
    }
}
