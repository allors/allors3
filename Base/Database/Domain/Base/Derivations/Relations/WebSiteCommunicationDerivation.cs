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

    public class WebSiteCommunicationsCreationDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("F960FDF6-8C3F-4D0F-9E41-48A30CB115F8");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.WebSiteCommunication.Class),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var webSiteCommunication in matches.Cast<WebSiteCommunication>())
            {
                webSiteCommunication.WorkItemDescription = $"Access website of {webSiteCommunication.ToParty.PartyName} about {webSiteCommunication.Subject}";
            }
        }
    }
}
