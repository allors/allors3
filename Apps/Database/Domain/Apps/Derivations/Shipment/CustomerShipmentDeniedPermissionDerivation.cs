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

    public class CustomerShipmentDeniedPermissionDerivation : DomainDerivation
    {
        public CustomerShipmentDeniedPermissionDerivation(M m) : base(m, new Guid("1121e021-7483-47ec-b8cf-1030e5dec9c3")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.CustomerShipment.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<CustomerShipment>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}