// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class WebSiteCommunicationsRule : Rule
    {
        public WebSiteCommunicationsRule(MetaPopulation m) : base(m, new Guid("F960FDF6-8C3F-4D0F-9E41-48A30CB115F8")) =>
            this.Patterns = new Pattern[]
            {
                m.WebSiteCommunication.RolePattern(v => v.Subject),
                m.WebSiteCommunication.RolePattern(v => v.ToParty),
                m.Party.RolePattern(v => v.DisplayName, v => v.CommunicationEventsWhereToParty, m.WebSiteCommunication),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WebSiteCommunication>())
            {
                @this.WorkItemDescription = $"Access website of {@this.ToParty?.DisplayName} about {@this.Subject}";
            }
        }
    }
}
