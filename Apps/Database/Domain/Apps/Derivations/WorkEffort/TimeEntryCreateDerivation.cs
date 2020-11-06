// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public class TimeEntryCreateDerivation : DomainDerivation
    {
        public TimeEntryCreateDerivation(M m) : base(m, new Guid("5bdddce9-6ec8-41c8-a321-33b7d44089e0")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.TimeEntry.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<TimeEntry>())
            {
                if (!@this.ExistBillingFrequency)
                {
                    @this.BillingFrequency = new TimeFrequencies(@this.Strategy.Session).Hour;
                }

                if (!@this.ExistTimeFrequency)
                {
                    @this.TimeFrequency = new TimeFrequencies(@this.Strategy.Session).Hour;
                }

                if (!@this.ExistIsBillable)
                {
                    @this.IsBillable = true;
                }

                var useInternalRate = @this.WorkEffort?.Customer is Organisation organisation && organisation.IsInternalOrganisation;

                if (!@this.ExistRateType)
                {
                    @this.RateType = useInternalRate ? new RateTypes(@this.Session()).InternalRate : new RateTypes(@this.Session()).StandardRate;
                }

                if (!@this.ExistBillingFrequency)
                {
                    @this.BillingFrequency = new TimeFrequencies(@this.Strategy.Session).Hour;
                }

                if (!@this.ExistTimeFrequency)
                {
                    @this.TimeFrequency = new TimeFrequencies(@this.Strategy.Session).Hour;
                }

                if (!@this.ExistIsBillable)
                {
                    @this.IsBillable = true;
                }
            }
        }
    }
}
