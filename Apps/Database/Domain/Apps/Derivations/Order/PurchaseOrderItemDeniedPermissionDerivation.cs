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

    public class PurchaseOrderItemDeniedPermissionDerivation : DomainDerivation
    {
        public PurchaseOrderItemDeniedPermissionDerivation(M m) : base(m, new Guid("68b556f7-00ae-49a7-8d51-49c52ae18b4d")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.PurchaseOrderItem.TransitionalDeniedPermissions),
            new ChangedPattern(this.M.OrderItemBilling.OrderItem) { Steps = new IPropertyType[] { this.M.OrderItemBilling.OrderItem}},
            new ChangedPattern(this.M.OrderRequirementCommitment.OrderItem) { Steps = new IPropertyType[] { this.M.OrderRequirementCommitment.OrderItem}},
            new ChangedPattern(this.M.WorkEffort.OrderItemFulfillment) { Steps = new IPropertyType[] { this.M.WorkEffort.OrderItemFulfillment}},
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrderItem>())
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
