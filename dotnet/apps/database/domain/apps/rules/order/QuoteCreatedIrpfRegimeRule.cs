// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations.Rules;
    using Meta;

    public class QuoteCreatedIrpfRegimeRule : Rule
    {
        public QuoteCreatedIrpfRegimeRule(MetaPopulation m) : base(m, new Guid("b66c0721-4aa5-4ca7-91a0-534f6cfc6718")) =>
            this.Patterns = new Pattern[]
            {
                m.Quote.RolePattern(v => v.QuoteState),
                m.Quote.RolePattern(v => v.Issuer),
                m.Quote.RolePattern(v => v.Receiver),
                m.Quote.RolePattern(v => v.AssignedIrpfRegime),
                m.Quote.RolePattern(v => v.IssueDate),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Quote>().Where(v => v.QuoteState.IsCreated))
            {
                @this.DerivedIrpfRegime = @this.AssignedIrpfRegime;

                if (@this.ExistIssueDate)
                {
                    @this.DerivedIrpfRate = @this.DerivedIrpfRegime?.IrpfRates.First(v => v.FromDate <= @this.IssueDate && (!v.ExistThroughDate || v.ThroughDate >= @this.IssueDate));
                }
            }
        }
    }
}
