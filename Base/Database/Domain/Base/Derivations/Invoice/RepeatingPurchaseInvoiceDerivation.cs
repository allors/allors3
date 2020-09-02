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

    public class RepeatingPurchaseInvoiceDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("BEC1F9FD-71CF-4B74-BF40-CDA30AB4C3FB");

        public IEnumerable<Pattern> Patterns { get; } = new[]
        {
            new CreatedPattern(M.RepeatingPurchaseInvoice.Class)
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var repeatingPurchaseInvoice in matches.Cast<RepeatingPurchaseInvoice>())
            {
                if (!repeatingPurchaseInvoice.Frequency.Equals(new TimeFrequencies(repeatingPurchaseInvoice.Strategy.Session).Month) && !repeatingPurchaseInvoice.Frequency.Equals(new TimeFrequencies(repeatingPurchaseInvoice.Strategy.Session).Week))
                {
                    validation.AddError($"{repeatingPurchaseInvoice} {M.RepeatingPurchaseInvoice.Frequency} {ErrorMessages.FrequencyNotSupported}");
                }

                if (repeatingPurchaseInvoice.Frequency.Equals(new TimeFrequencies(repeatingPurchaseInvoice.Strategy.Session).Week) && !repeatingPurchaseInvoice.ExistDayOfWeek)
                {
                    validation.AssertExists(repeatingPurchaseInvoice, M.RepeatingPurchaseInvoice.DayOfWeek);
                }

                if (repeatingPurchaseInvoice.Frequency.Equals(new TimeFrequencies(repeatingPurchaseInvoice.Strategy.Session).Month) && repeatingPurchaseInvoice.ExistDayOfWeek)
                {
                    validation.AssertNotExists(repeatingPurchaseInvoice, M.RepeatingPurchaseInvoice.DayOfWeek);
                }

                if (repeatingPurchaseInvoice.Frequency.Equals(new TimeFrequencies(repeatingPurchaseInvoice.Strategy.Session).Week) && repeatingPurchaseInvoice.ExistDayOfWeek && repeatingPurchaseInvoice.ExistNextExecutionDate)
                {
                    if (!repeatingPurchaseInvoice.NextExecutionDate.DayOfWeek.ToString().Equals(repeatingPurchaseInvoice.DayOfWeek.Name))
                    {
                        validation.AddError($"{repeatingPurchaseInvoice} {M.RepeatingPurchaseInvoice.DayOfWeek} {ErrorMessages.DateDayOfWeek}");
                    }
                }
            }
        }
    }
}
