// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class TransferDeniedPermissionDerivation : DomainDerivation
    {
        public TransferDeniedPermissionDerivation(M m) : base(m, new Guid("d1c10c75-b65b-4410-a700-a54f898310d1")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.Transfer.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Transfer>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}
