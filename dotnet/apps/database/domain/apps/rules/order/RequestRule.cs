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

    public class RequestRule : Rule
    {
        public RequestRule(MetaPopulation m) : base(m, new Guid("AF5D09BF-9ACF-4C29-9445-6D24BE2F04E6")) =>
            this.Patterns = new Pattern[]
            {
                m.Request.RolePattern(v => v.Recipient),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<Request>())
            {
                if (@this.ExistRecipient && !@this.ExistRequestNumber)
                {
                    var year = @this.RequestDate.Year;
                    @this.RequestNumber = @this.Recipient.NextRequestNumber(year);

                    var fiscalYearInternalOrganisationSequenceNumbers = @this.Recipient.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = @this.Recipient.RequestSequence.IsEnforcedSequence ? @this.Recipient.RequestNumberPrefix : fiscalYearInternalOrganisationSequenceNumbers.RequestNumberPrefix;
                    @this.SortableRequestNumber = @this.Transaction().GetSingleton().SortableNumber(prefix, @this.RequestNumber, year.ToString());
                }
            }
        }
    }
}
