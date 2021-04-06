// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;

    public class RequestDeriveCurrencyRule : Rule
    {
        public RequestDeriveCurrencyRule(MetaPopulation m) : base(m, new Guid("aea23418-c816-44ad-9c49-e82edd65dfc3")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Request, m.Request.Recipient),
                new RolePattern(m.Request, m.Request.Originator),
                new RolePattern(m.Request, m.Request.AssignedCurrency),
                new RolePattern(m.Party, m.Party.PreferredCurrency) { Steps = new IPropertyType[] { this.M.Party.RequestsWhereOriginator}},
                new RolePattern(m.Organisation, m.Organisation.PreferredCurrency) { Steps = new IPropertyType[] { this.M.Organisation.RequestsWhereRecipient}},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Request>())
            {
                @this.DerivedCurrency = @this.AssignedCurrency ?? @this.Originator?.PreferredCurrency ?? @this.Recipient?.PreferredCurrency;
            }
        }
    }
}
