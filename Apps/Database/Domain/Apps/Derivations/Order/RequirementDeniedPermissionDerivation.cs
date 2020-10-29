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

    public class RequirementDeniedPermissionDerivation : DomainDerivation
    {
        public RequirementDeniedPermissionDerivation(M m) : base(m, new Guid("f0023baa-b40f-4840-8f8f-db90304809e5")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.Requirement.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Requirement>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}
