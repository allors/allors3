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
        public class RepeatingSalesInvoiceCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdRepeatingSalesInvoices = changeSet.Created.Select(v=>v.GetObject()).OfType<RepeatingSalesInvoice>();

                foreach(var repeatingSalesInvoice in createdRepeatingSalesInvoices)
                {
                    if (!repeatingSalesInvoice.Frequency.Equals(new TimeFrequencies(repeatingSalesInvoice.Strategy.Session).Month) && !repeatingSalesInvoice.Frequency.Equals(new TimeFrequencies(repeatingSalesInvoice.Strategy.Session).Week))
                    {
                        validation.AddError($"{repeatingSalesInvoice} {M.RepeatingSalesInvoice.Frequency} {ErrorMessages.FrequencyNotSupported}");
                    }

                    if (repeatingSalesInvoice.Frequency.Equals(new TimeFrequencies(repeatingSalesInvoice.Strategy.Session).Week) && !repeatingSalesInvoice.ExistDayOfWeek)
                    {
                       validation.AssertExists(repeatingSalesInvoice, M.RepeatingSalesInvoice.DayOfWeek);
                    }

                    if (repeatingSalesInvoice.Frequency.Equals(new TimeFrequencies(repeatingSalesInvoice.Strategy.Session).Month) && repeatingSalesInvoice.ExistDayOfWeek)
                    {
                        validation.AssertNotExists(repeatingSalesInvoice, M.RepeatingSalesInvoice.DayOfWeek);
                    }

                    if (repeatingSalesInvoice.Frequency.Equals(new TimeFrequencies(repeatingSalesInvoice.Strategy.Session).Week) && repeatingSalesInvoice.ExistDayOfWeek && repeatingSalesInvoice.ExistNextExecutionDate)
                    {
                        if (!repeatingSalesInvoice.NextExecutionDate.DayOfWeek.ToString().Equals(repeatingSalesInvoice.DayOfWeek.Name))
                        {
                           validation.AddError($"{repeatingSalesInvoice} {M.RepeatingSalesInvoice.DayOfWeek} {ErrorMessages.DateDayOfWeek}");
                        }
                    }
                }

            }
        }

        public static void RepeatingSalesInvoiceRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("db0d398e-8326-42d1-8c09-37f55fcfcc28")] = new RepeatingSalesInvoiceCreationDerivation();
        }
    }
}
