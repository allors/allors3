// <copyright file="TimeEntry.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;

    public partial class TimeEntry
    {
        public decimal ActualHours
        {
            get
            {
                var frequencies = new TimeFrequencies(this.Strategy.Session);

                var through = this.ExistThroughDate ? this.ThroughDate : this.Session().Now();
                var minutes = (decimal)(through - this.FromDate).Value.TotalMinutes;
                var hours = (decimal)frequencies.Minute.ConvertToFrequency((decimal)minutes, frequencies.Hour);

                return Math.Round(hours, this.DecimalScale);
            }
        }

        private int DecimalScale => this.Meta.AmountOfTime.Scale ?? 2;

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistBillingFrequency)
            {
                this.BillingFrequency = new TimeFrequencies(this.Strategy.Session).Hour;
            }

            if (!this.ExistTimeFrequency)
            {
                this.TimeFrequency = new TimeFrequencies(this.Strategy.Session).Hour;
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
                this.RateType = useInternalRate ? new RateTypes(this.Session()).InternalRate : new RateTypes(this.Session()).StandardRate;
            }

            if (!this.ExistBillingFrequency)
            {
                this.BillingFrequency = new TimeFrequencies(this.Strategy.Session).Hour;
            }

            if (!this.ExistTimeFrequency)
            {
                this.TimeFrequency = new TimeFrequencies(this.Strategy.Session).Hour;
            }

            if (!this.ExistIsBillable)
            {
                this.IsBillable = true;
            }
        }

        public void AppsOnPreDerive(ObjectOnPreDerive method)
        {
            var (iteration, changeSet, derivedObjects) = method;

            if (iteration.IsMarked(this) || changeSet.IsCreated(this) || changeSet.HasChangedRoles(this))
            {
                if (this.ExistWorkEffort)
                {
                    iteration.AddDependency(this.WorkEffort, this);
                    iteration.Mark(this.WorkEffort);
                }
            }
        }
        public void AppsDelegateAccess(DelegatedAccessControlledObjectDelegateAccess method)
        {
            if (method.SecurityTokens == null)
            {
                var workEffortSecurityTokens = this.WorkEffort?.SecurityTokens ?? Array.Empty<SecurityToken>();
                method.SecurityTokens = workEffortSecurityTokens.Append(this.Worker?.OwnerSecurityToken).ToArray();
            }

            if (method.DeniedPermissions == null)
            {
                method.DeniedPermissions = this.WorkEffort?.DeniedPermissions.ToArray();
            }
        }
    }
}
