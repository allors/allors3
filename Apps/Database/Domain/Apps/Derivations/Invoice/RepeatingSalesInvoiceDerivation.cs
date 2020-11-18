// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Domain.Derivations;
    using Allors.Database.Meta;
    using Database.Derivations;
    using Resources;

    public class RepeatingSalesInvoiceDerivation : DomainDerivation
    {
        public RepeatingSalesInvoiceDerivation(M m) : base(m, new Guid("BEC1F9FD-71CF-4B74-BF40-CDA30AB4C3FB")) =>
            this.Patterns = new[]
            {
                new ChangedPattern(this.M.RepeatingSalesInvoice.Frequency)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RepeatingSalesInvoice>())
            {
                if (@this.ExistFrequency)
                {
                    if (!@this.Frequency.Equals(new TimeFrequencies(@this.Strategy.Session).Month) && !@this.Frequency.Equals(new TimeFrequencies(@this.Strategy.Session).Week))
                    {
                        validation.AddError($"{@this} {this.M.RepeatingSalesInvoice.Frequency} {ErrorMessages.FrequencyNotSupported}");
                    }

                    if (@this.Frequency.Equals(new TimeFrequencies(@this.Strategy.Session).Week) && !@this.ExistDayOfWeek)
                    {
                        validation.AssertExists(@this, this.M.RepeatingSalesInvoice.DayOfWeek);
                    }

                    if (@this.Frequency.Equals(new TimeFrequencies(@this.Strategy.Session).Month) && @this.ExistDayOfWeek)
                    {
                        validation.AssertNotExists(@this, this.M.RepeatingSalesInvoice.DayOfWeek);
                    }

                    if (@this.Frequency.Equals(new TimeFrequencies(@this.Strategy.Session).Week) && @this.ExistDayOfWeek && @this.ExistNextExecutionDate)
                    {
                        if (!@this.NextExecutionDate.DayOfWeek.ToString().Equals(@this.DayOfWeek.Name))
                        {
                            validation.AddError($"{@this} {this.M.RepeatingSalesInvoice.DayOfWeek} {ErrorMessages.DateDayOfWeek}");
                        }
                    }
                }
            }
        }
    }
}
