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

    public class QuoteDerivation : DomainDerivation
    {
        public QuoteDerivation(M m) : base(m, new Guid("B2464D89-5370-44D7-BB6B-7E6FA48EEF0B")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.Quote.Issuer),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Quote>())
            {
                if (!@this.ExistQuoteNumber && @this.ExistIssuer)
                {
                    var year = @this.IssueDate.Year;
                    @this.QuoteNumber = @this.Issuer.NextQuoteNumber(year);

                    var fiscalYearsInternalOrganisationSequenceNumbers = new FiscalYearsInternalOrganisationSequenceNumbers(@this.Session()).Extent();
                    fiscalYearsInternalOrganisationSequenceNumbers.Filter.AddEquals(M.FiscalYearInternalOrganisationSequenceNumbers.FiscalYear, year);
                    var fiscalYearInternalOrganisationSequenceNumbers = fiscalYearsInternalOrganisationSequenceNumbers.First;

                    var prefix = fiscalYearInternalOrganisationSequenceNumbers == null ? @this.Issuer.QuoteNumberPrefix : fiscalYearInternalOrganisationSequenceNumbers.QuoteNumberPrefix;
                    @this.SortableQuoteNumber = @this.Session().GetSingleton().SortableNumber(prefix, @this.QuoteNumber, year.ToString());
                }

                @this.AddSecurityToken(new SecurityTokens(cycle.Session).DefaultSecurityToken);
            }
        }
    }
}
