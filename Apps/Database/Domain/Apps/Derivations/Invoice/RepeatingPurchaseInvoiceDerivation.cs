// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class RepeatingPurchaseInvoiceDerivation : DomainDerivation
    {
        public RepeatingPurchaseInvoiceDerivation(M m) : base(m, new Guid("BCFAE9E0-8100-4BD0-9262-14A56E557B57")) =>
            this.Patterns = new[]
            {
                new AssociationPattern(m.RepeatingPurchaseInvoice.Frequency),
                new AssociationPattern(m.RepeatingPurchaseInvoice.DayOfWeek),
                new AssociationPattern(m.RepeatingPurchaseInvoice.NextExecutionDate)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RepeatingPurchaseInvoice>())
            {
                if (!@this.Frequency.Equals(new TimeFrequencies(@this.Strategy.Transaction).Month) && !@this.Frequency.Equals(new TimeFrequencies(@this.Strategy.Transaction).Week))
                {
                    validation.AddError($"{@this} {this.M.RepeatingPurchaseInvoice.Frequency} {ErrorMessages.FrequencyNotSupported}");
                }

                if (@this.Frequency.Equals(new TimeFrequencies(@this.Strategy.Transaction).Week) && !@this.ExistDayOfWeek)
                {
                    validation.AssertExists(@this, this.M.RepeatingPurchaseInvoice.DayOfWeek);
                }

                if (@this.Frequency.Equals(new TimeFrequencies(@this.Strategy.Transaction).Month) && @this.ExistDayOfWeek)
                {
                    validation.AssertNotExists(@this, this.M.RepeatingPurchaseInvoice.DayOfWeek);
                }

                if (@this.Frequency.Equals(new TimeFrequencies(@this.Strategy.Transaction).Week) && @this.ExistDayOfWeek && @this.ExistNextExecutionDate)
                {
                    if (!@this.NextExecutionDate.DayOfWeek.ToString().Equals(@this.DayOfWeek.Name))
                    {
                        validation.AddError($"{@this} {this.M.RepeatingPurchaseInvoice.DayOfWeek} {ErrorMessages.DateDayOfWeek}");
                    }
                }
            }
        }
    }
}
