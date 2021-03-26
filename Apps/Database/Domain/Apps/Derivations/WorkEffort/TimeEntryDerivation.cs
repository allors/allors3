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

    public class TimeEntryDerivation : DomainDerivation
    {
        public TimeEntryDerivation(M m) : base(m, new Guid("fcacc37e-581a-4c6f-bb77-d06a2987ebcf")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.TimeEntry, m.TimeEntry.WorkEffort),
            new RolePattern(m.TimeEntry, m.TimeEntry.Worker),
            new RolePattern(m.TimeEntry, m.TimeEntry.RateType),
            new RolePattern(m.TimeEntry, m.TimeEntry.TimeFrequency),
            new RolePattern(m.TimeEntry, m.TimeEntry.BillingFrequency),
            new RolePattern(m.TimeEntry, m.TimeEntry.AssignedBillingRate),
            new RolePattern(m.TimeEntry, m.TimeEntry.FromDate),
            new RolePattern(m.TimeEntry, m.TimeEntry.ThroughDate),
            new RolePattern(m.TimeEntry, m.TimeEntry.AssignedAmountOfTime),
            new RolePattern(m.TimeEntry, m.TimeEntry.BillableAmountOfTime),
            new RolePattern(m.WorkEffort, m.WorkEffort.Customer) { Steps = new IPropertyType[] { m.WorkEffort.ServiceEntriesWhereWorkEffort }, OfType = m.TimeEntry.Class },
            new RolePattern(m.WorkEffort, m.WorkEffort.ExecutedBy) { Steps = new IPropertyType[] { m.WorkEffort.ServiceEntriesWhereWorkEffort }, OfType = m.TimeEntry.Class },
            new AssociationPattern(m.TimeSheet.TimeEntries),
            new AssociationPattern(m.WorkEffortAssignmentRate.WorkEffort) { Steps = new IPropertyType[] { m.WorkEffort.ServiceEntriesWhereWorkEffort }, OfType = m.TimeEntry.Class },
            new RolePattern(m.WorkEffortAssignmentRate, m.WorkEffortAssignmentRate.RateType) { Steps = new IPropertyType[] { m.WorkEffortAssignmentRate.WorkEffort, m.WorkEffort.ServiceEntriesWhereWorkEffort }, OfType = m.TimeEntry.Class },
            new RolePattern(m.WorkEffortAssignmentRate, m.WorkEffortAssignmentRate.Frequency) { Steps = new IPropertyType[] { m.WorkEffortAssignmentRate.WorkEffort, m.WorkEffort.ServiceEntriesWhereWorkEffort }, OfType = m.TimeEntry.Class },
            new RolePattern(m.WorkEffortAssignmentRate, m.WorkEffortAssignmentRate.Rate) { Steps = new IPropertyType[] { m.WorkEffortAssignmentRate.WorkEffort, m.WorkEffort.ServiceEntriesWhereWorkEffort }, OfType = m.TimeEntry.Class },
            new RolePattern(m.Party, m.Party.PartyRates) { Steps = new IPropertyType[] { m.Party.WorkEffortsWhereCustomer, m.WorkEffort.ServiceEntriesWhereWorkEffort }, OfType = m.TimeEntry.Class },
            new RolePattern(m.PartyRate, m.PartyRate.RateType) { Steps = new IPropertyType[] { m.PartyRate.PartyWherePartyRate, m.Party.WorkEffortsWhereCustomer, m.WorkEffort.ServiceEntriesWhereWorkEffort }, OfType = m.TimeEntry.Class },
            new RolePattern(m.PartyRate, m.PartyRate.Frequency) { Steps = new IPropertyType[] { m.PartyRate.PartyWherePartyRate, m.Party.WorkEffortsWhereCustomer, m.WorkEffort.ServiceEntriesWhereWorkEffort }, OfType = m.TimeEntry.Class },
            new RolePattern(m.PartyRate, m.PartyRate.Rate) { Steps = new IPropertyType[] { m.PartyRate.PartyWherePartyRate, m.Party.WorkEffortsWhereCustomer, m.WorkEffort.ServiceEntriesWhereWorkEffort }, OfType = m.TimeEntry.Class },
            new RolePattern(m.PartyRate, m.PartyRate.FromDate) { Steps = new IPropertyType[] { m.PartyRate.PartyWherePartyRate, m.Party.WorkEffortsWhereCustomer, m.WorkEffort.ServiceEntriesWhereWorkEffort }, OfType = m.TimeEntry.Class },
            new RolePattern(m.PartyRate, m.PartyRate.ThroughDate) { Steps = new IPropertyType[] { m.PartyRate.PartyWherePartyRate, m.Party.WorkEffortsWhereCustomer, m.WorkEffort.ServiceEntriesWhereWorkEffort }, OfType = m.TimeEntry.Class },
            new RolePattern(m.Party, m.Party.PartyRates) { Steps = new IPropertyType[] { m.Person.TimeEntriesWhereWorker }, OfType = m.TimeEntry.Class },
            new RolePattern(m.PartyRate, m.PartyRate.RateType) { Steps = new IPropertyType[] { m.PartyRate.PartyWherePartyRate, m.Person.TimeEntriesWhereWorker } },
            new RolePattern(m.PartyRate, m.PartyRate.Frequency) { Steps = new IPropertyType[] { m.PartyRate.PartyWherePartyRate, m.Person.TimeEntriesWhereWorker } },
            new RolePattern(m.PartyRate, m.PartyRate.Rate) { Steps = new IPropertyType[] { m.PartyRate.PartyWherePartyRate, m.Person.TimeEntriesWhereWorker } },
            new RolePattern(m.PartyRate, m.PartyRate.FromDate) { Steps = new IPropertyType[] { m.PartyRate.PartyWherePartyRate, m.Person.TimeEntriesWhereWorker } },
            new RolePattern(m.PartyRate, m.PartyRate.ThroughDate) { Steps = new IPropertyType[] { m.PartyRate.PartyWherePartyRate, m.Person.TimeEntriesWhereWorker } },
            new RolePattern(m.Party, m.Party.PartyRates) { Steps = new IPropertyType[] { m.Organisation.WorkEffortsWhereExecutedBy, m.WorkEffort.ServiceEntriesWhereWorkEffort }, OfType = m.TimeEntry.Class },
            new RolePattern(m.PartyRate, m.PartyRate.RateType) { Steps = new IPropertyType[] { m.PartyRate.PartyWherePartyRate, m.Organisation.WorkEffortsWhereExecutedBy, m.WorkEffort.ServiceEntriesWhereWorkEffort }, OfType = m.TimeEntry.Class },
            new RolePattern(m.PartyRate, m.PartyRate.Frequency) { Steps = new IPropertyType[] { m.PartyRate.PartyWherePartyRate, m.Organisation.WorkEffortsWhereExecutedBy, m.WorkEffort.ServiceEntriesWhereWorkEffort }, OfType = m.TimeEntry.Class },
            new RolePattern(m.PartyRate, m.PartyRate.Rate) { Steps = new IPropertyType[] { m.PartyRate.PartyWherePartyRate, m.Organisation.WorkEffortsWhereExecutedBy, m.WorkEffort.ServiceEntriesWhereWorkEffort }, OfType = m.TimeEntry.Class },
            new RolePattern(m.PartyRate, m.PartyRate.FromDate) { Steps = new IPropertyType[] { m.PartyRate.PartyWherePartyRate, m.Organisation.WorkEffortsWhereExecutedBy, m.WorkEffort.ServiceEntriesWhereWorkEffort }, OfType = m.TimeEntry.Class },
            new RolePattern(m.PartyRate, m.PartyRate.ThroughDate) { Steps = new IPropertyType[] { m.PartyRate.PartyWherePartyRate, m.Organisation.WorkEffortsWhereExecutedBy, m.WorkEffort.ServiceEntriesWhereWorkEffort }, OfType = m.TimeEntry.Class },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<TimeEntry>())
            {
                var useInternalRate = @this.WorkEffort?.Customer is Organisation organisation && organisation.IsInternalOrganisation;
                var rateType = useInternalRate ? new RateTypes(@this.Transaction()).InternalRate : @this.RateType;

                if (@this.ExistTimeSheetWhereTimeEntry)
                {
                    @this.Worker = @this.TimeSheetWhereTimeEntry.Worker;
                }

                //if (this.ExistWorker)
                //{
                //    var otherActiveTimeEntry = this.Worker.TimeEntriesWhereWorker.FirstOrDefault(v =>
                //                    v.Id != this.Id
                //                    && ((v.FromDate < this.FromDate && (!v.ExistThroughDate || v.ThroughDate > this.FromDate))
                //                        || (v.FromDate > this.FromDate && v.FromDate < this.ThroughDate)));

                //    if (otherActiveTimeEntry != null)
                //    {
                //        derivation.Validation.AddError(this, this.Meta.Worker, ErrorMessages.WorkerActiveTimeEntry.Replace("{0}", otherActiveTimeEntry.WorkEffort?.WorkEffortNumber));
                //    }
                //}

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
                    billingRate = Math.Round(billingRate * (1 + transaction.GetSingleton().Settings.InternalLabourSurchargePercentage / 100), 2);
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
                        @this.AmountOfTime = Math.Round((decimal)amount, 2);
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
                        @this.AmountOfTime = Math.Round((decimal)amount, 2);
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

                    var billableTimeInTimeEntryRateFrequency = Math.Round((decimal)frequencies.Minute.ConvertToFrequency(@this.BillableAmountOfTimeInMinutes, @this.BillingFrequency), 2);

                    @this.BillingAmount = Math.Round((decimal)(@this.BillingRate * billableTimeInTimeEntryRateFrequency), 2);

                    var timeSpendInTimeEntryRateFrequency = Math.Round((decimal)frequencies.Minute.ConvertToFrequency(@this.AmountOfTimeInMinutes, @this.BillingFrequency), 2);
                    @this.Cost = Math.Round((decimal)(costRate * timeSpendInTimeEntryRateFrequency), 2);
                }

            }
        }
    }
}
