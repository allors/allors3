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

    public class PartSpecificationDeniedPermissionDerivation : DomainDerivation
    {
        public PartSpecificationDeniedPermissionDerivation(M m) : base(m, new Guid("6764a4ea-6335-4252-8957-93b352df8d7b")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.PartSpecification, m.PartSpecification.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PartSpecification>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}
