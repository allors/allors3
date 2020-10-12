// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public class RepeatingSalesInvoiceDerivation : DomainDerivation
    {
        public RepeatingSalesInvoiceDerivation(M m) : base(m, new Guid("BEC1F9FD-71CF-4B74-BF40-CDA30AB4C3FB")) =>
            this.Patterns = new[]
            {
                new CreatedPattern(this.M.RepeatingSalesInvoice.Class)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var repeatingSalesInvoice in matches.Cast<RepeatingSalesInvoice>())
            {
                if (!repeatingSalesInvoice.Frequency.Equals(new TimeFrequencies(repeatingSalesInvoice.Strategy.Session).Month) && !repeatingSalesInvoice.Frequency.Equals(new TimeFrequencies(repeatingSalesInvoice.Strategy.Session).Week))
                {
                    validation.AddError($"{repeatingSalesInvoice} {this.M.RepeatingSalesInvoice.Frequency} {ErrorMessages.FrequencyNotSupported}");
                }

                if (repeatingSalesInvoice.Frequency.Equals(new TimeFrequencies(repeatingSalesInvoice.Strategy.Session).Week) && !repeatingSalesInvoice.ExistDayOfWeek)
                {
                    validation.AssertExists(repeatingSalesInvoice, this.M.RepeatingSalesInvoice.DayOfWeek);
                }

                if (repeatingSalesInvoice.Frequency.Equals(new TimeFrequencies(repeatingSalesInvoice.Strategy.Session).Month) && repeatingSalesInvoice.ExistDayOfWeek)
                {
                    validation.AssertNotExists(repeatingSalesInvoice, this.M.RepeatingSalesInvoice.DayOfWeek);
                }

                if (repeatingSalesInvoice.Frequency.Equals(new TimeFrequencies(repeatingSalesInvoice.Strategy.Session).Week) && repeatingSalesInvoice.ExistDayOfWeek && repeatingSalesInvoice.ExistNextExecutionDate)
                {
                    if (!repeatingSalesInvoice.NextExecutionDate.DayOfWeek.ToString().Equals(repeatingSalesInvoice.DayOfWeek.Name))
                    {
                        validation.AddError($"{repeatingSalesInvoice} {this.M.RepeatingSalesInvoice.DayOfWeek} {ErrorMessages.DateDayOfWeek}");
                    }
                }
            }

        }
    }
}
