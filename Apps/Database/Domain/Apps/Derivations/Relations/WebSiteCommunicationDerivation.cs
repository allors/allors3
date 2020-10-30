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

    public class WebSiteCommunicationsDerivation : DomainDerivation
    {
        public WebSiteCommunicationsDerivation(M m) : base(m, new Guid("F960FDF6-8C3F-4D0F-9E41-48A30CB115F8")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.WebSiteCommunication.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WebSiteCommunication>())
            {
                @this.WorkItemDescription = $"Access website of {@this.ToParty.PartyName} about {@this.Subject}";
            }
        }
    }
}
