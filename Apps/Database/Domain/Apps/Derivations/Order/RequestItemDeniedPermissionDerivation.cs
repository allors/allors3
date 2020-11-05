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

    public class RequestItemDeniedPermissionDerivation : DomainDerivation
    {
        public RequestItemDeniedPermissionDerivation(M m) : base(m, new Guid("45bfa303-4cff-4e8f-889a-eac847d02849")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.RequestItem.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RequestItem>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;

                var deletePermission = new Permissions(@this.Strategy.Session).Get(@this.Meta.ObjectType, @this.Meta.Delete);
                if (@this.IsDeletable)
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
                }
            }
        }
    }
}
