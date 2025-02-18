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
    using Derivations.Rules;
    using Meta;

    public class QuoteCreatedIrpfRegimeRule : Rule
    {
        public QuoteCreatedIrpfRegimeRule(MetaPopulation m) : base(m, new Guid("6a55acfc-4545-4b7d-87a4-3eaf97d2ebd1")) =>
            this.Patterns = new Pattern[]
            {
                m.Quote.RolePattern(v => v.QuoteState),
                m.Quote.RolePattern(v => v.Issuer),
                m.Quote.RolePattern(v => v.Receiver),
                m.Quote.RolePattern(v => v.AssignedIrpfRegime),
                m.Quote.RolePattern(v => v.IssueDate),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
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
