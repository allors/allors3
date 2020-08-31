// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class RepeatingPurchaseInvoiceCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdRepeatingPurchaseInvoices = changeSet.Created.Select(v=>v.GetObject()).OfType<RepeatingPurchaseInvoice>();

                foreach(var repeatingPurchaseInvoice in createdRepeatingPurchaseInvoices)
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

        public static void RepeatingPurchaseInvoiceRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("67eb26f4-144d-44aa-beb1-3e79602f34b8")] = new RepeatingPurchaseInvoiceCreationDerivation();
        }
    }
}
