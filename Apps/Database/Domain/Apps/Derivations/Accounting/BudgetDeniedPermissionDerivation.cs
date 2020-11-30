// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class BudgetDeniedPermissionDerivation : DomainDerivation
    {
        public BudgetDeniedPermissionDerivation(M m) : base(m, new Guid("d0807b6c-a7c9-4bd5-a4eb-c84cadcd9a8f")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.Budget.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Budget>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}
