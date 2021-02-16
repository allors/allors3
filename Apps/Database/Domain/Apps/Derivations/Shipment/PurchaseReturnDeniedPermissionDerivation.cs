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

    public class PurchaseReturnDeniedPermissionDerivation : DomainDerivation
    {
        public PurchaseReturnDeniedPermissionDerivation(M m) : base(m, new Guid("73e1af05-a7b3-433c-8f52-15bc1370d1fe")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.PurchaseReturn.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseReturn>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}
