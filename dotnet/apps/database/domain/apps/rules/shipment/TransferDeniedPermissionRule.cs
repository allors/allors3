// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class TransferDeniedPermissionRule : Rule
    {
        public TransferDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("d1c10c75-b65b-4410-a700-a54f898310d1")) =>
            this.Patterns = new Pattern[]
        {
            m.Transfer.RolePattern(v => v.TransitionalDeniedPermissions),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Transfer>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}
