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
    using Derivations.Rules;
    using Meta;

    public class ProposalNumberRule : Rule
    {
        public ProposalNumberRule(MetaPopulation m) : base(m, new Guid("f95644e6-dfdd-48a1-82dd-6b46be0bdb45")) =>
            this.Patterns = new Pattern[]
            {
                m.Proposal.RolePattern(v => v.Issuer),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Proposal>())
            {
                if (!@this.ExistQuoteNumber && @this.ExistIssuer)
                {
                    var year = @this.IssueDate.Year;
                    @this.QuoteNumber = @this.Issuer.NextProductQuoteNumber(year);

                    var fiscalYearInternalOrganisationSequenceNumbers = @this.Issuer.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = @this.Issuer.QuoteSequence.IsEnforcedSequence ? @this.Issuer.ProductQuoteNumberPrefix : fiscalYearInternalOrganisationSequenceNumbers.ProductQuoteNumberPrefix;
                    @this.SortableQuoteNumber = @this.Transaction().GetSingleton().SortableNumber(prefix, @this.QuoteNumber, year.ToString());
                }
            }
        }
    }
}
