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

    public class RequestCurrencyRule : Rule
    {
        public RequestCurrencyRule(MetaPopulation m) : base(m, new Guid("aea23418-c816-44ad-9c49-e82edd65dfc3")) =>
            this.Patterns = new Pattern[]
            {
                m.Request.RolePattern(v => v.Recipient),
                m.Request.RolePattern(v => v.Originator),
                m.Request.RolePattern(v => v.AssignedCurrency),
                m.Party.RolePattern(v => v.PreferredCurrency, v => v.RequestsWhereOriginator),
                m.Organisation.RolePattern(v => v.PreferredCurrency, v => v.RequestsWhereRecipient),
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
