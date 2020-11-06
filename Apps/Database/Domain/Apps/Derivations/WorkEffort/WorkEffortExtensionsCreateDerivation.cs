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

    public class WorkEffortExtensionsCreateDerivation : DomainDerivation
    {
        public WorkEffortExtensionsCreateDerivation(M m) : base(m, new Guid("bd3be749-5569-46e4-9df0-5557f1197c36")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.WorkEffort.Interface),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<WorkEffort>())
            {
                if (!@this.ExistWorkEffortState)
                {
                    @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Session).Created;
                }

                if (!@this.ExistOwner && @this.Strategy.Session.State().User is Person owner)
                {
                    @this.Owner = owner;
                }
            }
        }
    }
}
