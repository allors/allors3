// <copyright file="TimeEntry.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class TimeEntry
    {
        public decimal ActualHours
        {
            get
            {
                var frequencies = new TimeFrequencies(this.Strategy.Transaction);

                var through = this.ExistThroughDate ? this.ThroughDate : this.Transaction().Now();
                var minutes = (decimal)(through - this.FromDate).Value.TotalMinutes;
                var hours = (decimal)frequencies.Minute.ConvertToFrequency(minutes, frequencies.Hour);

                return Rounder.RoundDecimal(hours, this.DecimalScale);
            }
        }

        private int DecimalScale => this.Meta.AmountOfTime.Scale ?? 2;

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistBillingFrequency)
            {
                this.BillingFrequency = new TimeFrequencies(this.Strategy.Transaction).Hour;
            }

            if (!this.ExistTimeFrequency)
            {
                this.TimeFrequency = new TimeFrequencies(this.Strategy.Transaction).Hour;
            }

            if (!this.ExistIsBillable)
            {
                this.IsBillable = true;
            }
        }

        public void AppsOnInit(ObjectOnInit method)
        {
            var useInternalRate = this.WorkEffort?.Customer is Organisation organisation && organisation.IsInternalOrganisation;

            if (!this.ExistRateType)
            {
                this.RateType = useInternalRate ? new RateTypes(this.Transaction()).InternalRate : new RateTypes(this.Transaction()).StandardRate;
            }
        }

        public void AppsOnPostDerive(ObjectOnPostDerive method)
        {
            method.Derivation.Validation.AssertExists(this, this.M.TimeEntry.Worker);
        }

        public void AppsDelete(DeletableDelete method) => this.WorkEffort.DerivationTrigger = Guid.NewGuid();
    }
}
