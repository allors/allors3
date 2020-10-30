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

    public class SerialisedInventoryItemDeniedPermissionDerivation : DomainDerivation
    {
        public SerialisedInventoryItemDeniedPermissionDerivation(M m) : base(m, new Guid("30ae162a-ac07-4a80-817c-1f5455976f93")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.SerialisedInventoryItem.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedInventoryItem>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}
