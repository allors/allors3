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
