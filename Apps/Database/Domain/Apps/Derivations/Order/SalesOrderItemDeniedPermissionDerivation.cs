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

    public class SalesOrderItemDeniedPermissionDerivation : DomainDerivation
    {
        public SalesOrderItemDeniedPermissionDerivation(M m) : base(m, new Guid("84ff5d0c-c15d-426d-a019-ad8ab0bdbcf2")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.SalesOrderItem.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesOrderItem>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}
