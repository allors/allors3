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
    using Derivations.Rules;
    using Meta;
    using DateTime = System.DateTime;

    public class TimeEntryRule : Rule
    {
        public TimeEntryRule(MetaPopulation m) : base(m, new Guid("fcacc37e-581a-4c6f-bb77-d06a2987ebcf")) =>
            this.Patterns = new Pattern[]
        {
            m.TimeEntry.RolePattern(v => v.WorkEffort),
            m.TimeEntry.RolePattern(v => v.Worker),
            m.TimeEntry.RolePattern(v => v.RateType),
            m.TimeEntry.RolePattern(v => v.TimeFrequency),
            m.TimeEntry.RolePattern(v => v.BillingFrequency),
            m.TimeEntry.RolePattern(v => v.AssignedBillingRate),
            m.TimeEntry.RolePattern(v => v.FromDate),
            m.TimeEntry.RolePattern(v => v.ThroughDate),
            m.TimeEntry.RolePattern(v => v.AssignedAmountOfTime),
            m.TimeEntry.RolePattern(v => v.BillableAmountOfTime),
            m.WorkEffort.RolePattern(v => v.Customer, v => v.ServiceEntriesWhereWorkEffort, m.TimeEntry),
            m.WorkEffort.RolePattern(v => v.ExecutedBy, v => v.ServiceEntriesWhereWorkEffort, m.TimeEntry),
            m.WorkEffort.AssociationPattern(v => v.WorkEffortAssignmentRatesWhereWorkEffort, v => v.ServiceEntriesWhereWorkEffort, m.TimeEntry),
            m.WorkEffortAssignmentRate.RolePattern(v => v.RateType, v => v.WorkEffort.WorkEffort.ServiceEntriesWhereWorkEffort, m.TimeEntry),
            m.WorkEffortAssignmentRate.RolePattern(v => v.Frequency, v => v.WorkEffort.WorkEffort.ServiceEntriesWhereWorkEffort, m.TimeEntry),
            m.WorkEffortAssignmentRate.RolePattern(v => v.Rate, v => v.WorkEffort.WorkEffort.ServiceEntriesWhereWorkEffort, m.TimeEntry),
            m.Party.RolePattern(v => v.PartyRates, v => v.WorkEffortsWhereCustomer.WorkEffort.ServiceEntriesWhereWorkEffort, m.TimeEntry),
            m.PartyRate.RolePattern(v => v.RateType, v => v.PartyWherePartyRate.Party.WorkEffortsWhereCustomer.WorkEffort.ServiceEntriesWhereWorkEffort, m.TimeEntry),
            m.PartyRate.RolePattern(v => v.Frequency, v => v.PartyWherePartyRate.Party.WorkEffortsWhereCustomer.WorkEffort.ServiceEntriesWhereWorkEffort, m.TimeEntry),
            m.PartyRate.RolePattern(v => v.Rate, v => v.PartyWherePartyRate.Party.WorkEffortsWhereCustomer.WorkEffort.ServiceEntriesWhereWorkEffort, m.TimeEntry),
            m.PartyRate.RolePattern(v => v.FromDate, v => v.PartyWherePartyRate.Party.WorkEffortsWhereCustomer.WorkEffort.ServiceEntriesWhereWorkEffort, m.TimeEntry),
            m.PartyRate.RolePattern(v => v.ThroughDate, v => v.PartyWherePartyRate.Party.WorkEffortsWhereCustomer.WorkEffort.ServiceEntriesWhereWorkEffort, m.TimeEntry),
            m.Party.RolePattern(v => v.PartyRates, v => v.AsPerson.TimeEntriesWhereWorker, m.TimeEntry),
            m.PartyRate.RolePattern(v => v.RateType, v => v.PartyWherePartyRate.Party.AsPerson.TimeEntriesWhereWorker),
            m.PartyRate.RolePattern(v => v.Frequency, v => v.PartyWherePartyRate.Party.AsPerson.TimeEntriesWhereWorker),
            m.PartyRate.RolePattern(v => v.Rate, v => v.PartyWherePartyRate.Party.AsPerson.TimeEntriesWhereWorker),
            m.PartyRate.RolePattern(v => v.FromDate, v => v.PartyWherePartyRate.Party.AsPerson.TimeEntriesWhereWorker),
            m.PartyRate.RolePattern(v => v.ThroughDate, v => v.PartyWherePartyRate.Party.AsPerson.TimeEntriesWhereWorker),
            m.Party.RolePattern(v => v.PartyRates, v => v.AsOrganisation.WorkEffortsWhereExecutedBy.WorkEffort.ServiceEntriesWhereWorkEffort, m.TimeEntry),
            m.PartyRate.RolePattern(v => v.RateType, v => v.PartyWherePartyRate.Party.AsOrganisation.WorkEffortsWhereExecutedBy.WorkEffort.ServiceEntriesWhereWorkEffort, m.TimeEntry),
            m.PartyRate.RolePattern(v => v.Frequency, v => v.PartyWherePartyRate.Party.AsOrganisation.WorkEffortsWhereExecutedBy.WorkEffort.ServiceEntriesWhereWorkEffort, m.TimeEntry),
            m.PartyRate.RolePattern(v => v.Rate, v => v.PartyWherePartyRate.Party.AsOrganisation.WorkEffortsWhereExecutedBy.WorkEffort.ServiceEntriesWhereWorkEffort, m.TimeEntry),
            m.PartyRate.RolePattern(v => v.FromDate, v => v.PartyWherePartyRate.Party.AsOrganisation.WorkEffortsWhereExecutedBy.WorkEffort.ServiceEntriesWhereWorkEffort, m.TimeEntry),
            m.PartyRate.RolePattern(v => v.ThroughDate, v => v.PartyWherePartyRate.Party.AsOrganisation.WorkEffortsWhereExecutedBy.WorkEffort.ServiceEntriesWhereWorkEffort, m.TimeEntry),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<TimeEntry>())
            {
                var useInternalRate = @this.WorkEffort?.Customer is Organisation organisation && organisation.IsInternalOrganisation;
                var rateType = useInternalRate ? new RateTypes(@this.Transaction()).InternalRate : @this.RateType;

                var billingRate = 0M;
                if (@this.AssignedBillingRate.HasValue)
                {
                    billingRate = @this.AssignedBillingRate.Value;
                }
                else
                {
                    if (@this.ExistWorkEffort)
                    {
                        var workEffortAssignmentRate = @this.WorkEffort.WorkEffortAssignmentRatesWhereWorkEffort.FirstOrDefault(v =>
                                                                v.ExistRateType
                                                                && v.ExistFrequency
                                                                && v.RateType.Equals(@this.RateType)
                                                                && v.Frequency.Equals(@this.BillingFrequency));
                        if (workEffortAssignmentRate != null)
                        {
                            billingRate = workEffortAssignmentRate.Rate;
                        }
                    }

                    if (billingRate == 0
                        && @this.ExistWorkEffort
                        && @this.WorkEffort.ExistCustomer
                        && (@this.WorkEffort.Customer as Organisation)?.IsInternalOrganisation == false)
                    {
                        var partyRate = @this.WorkEffort.Customer.PartyRates.FirstOrDefault(v =>
                                        v.ExistRateType
                                        && v.ExistFrequency
                                        && v.RateType.Equals(rateType)
                                        && v.Frequency.Equals(@this.BillingFrequency)
                                        && v.FromDate <= @this.FromDate && (!v.ExistThroughDate || v.ThroughDate >= @this.FromDate));
                        if (partyRate != null)
                        {
                            billingRate = partyRate.Rate;
                        }
                    }

                    if (billingRate == 0 && @this.ExistWorker && @this.ExistRateType)
                    {
                        var partyRate = @this.Worker.PartyRates.FirstOrDefault(v =>
                                        v.ExistRateType
                                        && v.ExistFrequency
                                        && v.RateType.Equals(rateType)
                                        && v.Frequency.Equals(@this.BillingFrequency)
                                        && v.FromDate <= @this.FromDate && (!v.ExistThroughDate || v.ThroughDate >= @this.FromDate));
                        if (partyRate != null)
                        {
                            billingRate = partyRate.Rate;
                        }
                    }

                    if (billingRate == 0 && @this.ExistWorkEffort && @this.WorkEffort.ExistExecutedBy)
                    {
                        var partyRate = @this.WorkEffort.ExecutedBy.PartyRates.FirstOrDefault(v =>
                                        v.ExistRateType
                                        && v.ExistFrequency
                                        && v.RateType.Equals(rateType)
                                        && v.Frequency.Equals(@this.BillingFrequency)
                                        && v.FromDate <= @this.FromDate && (!v.ExistThroughDate || v.ThroughDate >= @this.FromDate));
                        if (partyRate != null)
                        {
                            billingRate = partyRate.Rate;
                        }
                    }
                }

                // rate before uplift
                var costRate = billingRate;

                if (useInternalRate && !Equals(@this.WorkEffort.Customer, @this.WorkEffort.ExecutedBy))
                {
                    billingRate = Rounder.RoundDecimal(billingRate * (1 + transaction.GetSingleton().Settings.InternalLabourSurchargePercentage / 100), 2);
                }

                @this.BillingRate = billingRate;

                if (@this.ExistBillingRate)
                {
                    validation.AssertExists(@this, @this.Meta.BillingFrequency);
                }

                // calculate AmountOfTime Or ThroughDate
                var frequencies = new TimeFrequencies(@this.Strategy.Transaction);
                @this.AmountOfTime = null;

                if (@this.ThroughDate != null)
                {
                    var timeSpan = @this.ThroughDate - @this.FromDate;
                    @this.AmountOfTimeInMinutes = (decimal)timeSpan.Value.TotalMinutes;
                    var amount = frequencies.Minute.ConvertToFrequency(@this.AmountOfTimeInMinutes, @this.TimeFrequency);

                    if (amount == null)
                    {
                        @this.RemoveAmountOfTime();
                    }
                    else
                    {
                        @this.AmountOfTime = Rounder.RoundDecimal((decimal)amount, 2);
                    }
                }
                else if (@this.ExistAssignedAmountOfTime)
                {
                    @this.AmountOfTimeInMinutes = (decimal)@this.TimeFrequency.ConvertToFrequency((decimal)@this.AssignedAmountOfTime, frequencies.Minute);

                    var timeSpan = TimeSpan.FromMinutes((double)@this.AmountOfTimeInMinutes);
                    @this.ThroughDate = new DateTime(@this.FromDate.Ticks, @this.FromDate.Kind) + timeSpan;

                    @this.AmountOfTime = @this.AssignedAmountOfTime;
                }
                else
                {
                    var timeSpan = @this.Transaction().Now() - @this.FromDate;
                    @this.AmountOfTimeInMinutes = (decimal)timeSpan.TotalMinutes;
                    var amount = frequencies.Minute.ConvertToFrequency(@this.AmountOfTimeInMinutes, @this.TimeFrequency);

                    if (amount == null)
                    {
                        @this.RemoveAmountOfTime();
                    }
                    else
                    {
                        @this.AmountOfTime = Rounder.RoundDecimal((decimal)amount, 2);
                    }
                }

                if (@this.ExistBillingRate && @this.ExistBillingFrequency)
                {
                    if (@this.BillableAmountOfTime.HasValue)
                    {
                        @this.BillableAmountOfTimeInMinutes = (decimal)@this.TimeFrequency.ConvertToFrequency((decimal)@this.BillableAmountOfTime, frequencies.Minute);
                    }
                    else
                    {
                        @this.BillableAmountOfTimeInMinutes = @this.AmountOfTimeInMinutes;
                    }

                    var billableTimeInTimeEntryRateFrequency = Rounder.RoundDecimal((decimal)frequencies.Minute.ConvertToFrequency(@this.BillableAmountOfTimeInMinutes, @this.BillingFrequency), 2);

                    @this.BillingAmount = Rounder.RoundDecimal((decimal)(@this.BillingRate * billableTimeInTimeEntryRateFrequency), 2);

                    var timeSpendInTimeEntryRateFrequency = Rounder.RoundDecimal((decimal)frequencies.Minute.ConvertToFrequency(@this.AmountOfTimeInMinutes, @this.BillingFrequency), 2);
                    @this.Cost = Rounder.RoundDecimal((decimal)(costRate * timeSpendInTimeEntryRateFrequency), 2);
                }

            }
        }
    }
}
