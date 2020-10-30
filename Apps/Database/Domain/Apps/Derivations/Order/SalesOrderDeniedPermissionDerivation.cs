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

    public class SalesOrderDeniedPermissionDerivation : DomainDerivation
    {
        public SalesOrderDeniedPermissionDerivation(M m) : base(m, new Guid("6e383218-1d0f-41bb-83ed-7f6f3bf551ca")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.SalesOrder.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesOrder>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}
