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

    public class StatementOfWorkQuoteNumberRule : Rule
    {
        public StatementOfWorkQuoteNumberRule(MetaPopulation m) : base(m, new Guid("0a1306c8-e093-473f-ae6f-7e82cf37d095")) =>
            this.Patterns = new Pattern[]
            {
                m.StatementOfWork.RolePattern(v => v.Issuer),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<StatementOfWork>())
            {
                if (!@this.ExistQuoteNumber && @this.ExistIssuer)
                {
                    var year = @this.IssueDate.Year;
                    @this.QuoteNumber = @this.Issuer.NextStatementOfWorkNumber(year);

                    var fiscalYearInternalOrganisationSequenceNumbers = @this.Issuer.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = @this.Issuer.QuoteSequence.IsEnforcedSequence ? @this.Issuer.StatementOfWorkNumberPrefix : fiscalYearInternalOrganisationSequenceNumbers.StatementOfWorkNumberPrefix;
                    @this.SortableQuoteNumber = @this.Transaction().GetSingleton().SortableNumber(prefix, @this.QuoteNumber, year.ToString());
                }
            }
        }
    }
}
