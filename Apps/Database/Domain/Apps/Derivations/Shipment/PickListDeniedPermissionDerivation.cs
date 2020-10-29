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

    public class PickListDeniedPermissionDerivation : DomainDerivation
    {
        public PickListDeniedPermissionDerivation(M m) : base(m, new Guid("5650fa14-e2bb-4b7c-b08d-976b11994dea")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.PickList.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PickList>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}
