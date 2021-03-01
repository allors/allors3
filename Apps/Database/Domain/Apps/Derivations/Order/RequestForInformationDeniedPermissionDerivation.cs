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

    public class RequestForInformationDeniedPermissionDerivation : DomainDerivation
    {
        public RequestForInformationDeniedPermissionDerivation(M m) : base(m, new Guid("3227f658-588b-42eb-bf4f-f76d1d4b85c4")) =>
            this.Patterns = new Pattern[]
        {
            new AssociationPattern(m.RequestForInformation.TransitionalDeniedPermissions),
            new AssociationPattern(m.RequestForInformation.RequestItems),
            new AssociationPattern(m.RequestItem.RequestItemState) { Steps =  new IPropertyType[] { m.RequestItem.RequestWhereRequestItem}, OfType = m.RequestForInformation.Class },
            new RolePattern(m.Quote.Request) { OfType = m.RequestForInformation.Class },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RequestForInformation>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;

                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta.ObjectType, @this.Meta.Delete);
                if (@this.IsDeletable())
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
