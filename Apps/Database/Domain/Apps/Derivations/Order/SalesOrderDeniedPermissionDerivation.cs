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

    public class SalesOrderDeniedPermissionDerivation : DomainDerivation
    {
        public SalesOrderDeniedPermissionDerivation(M m) : base(m, new Guid("6e383218-1d0f-41bb-83ed-7f6f3bf551ca")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.SalesOrder.TransitionalDeniedPermissions),
            new ChangedPattern(this.M.SalesOrderItem.SalesOrderItemState) { Steps = new IPropertyType[] {m.SalesOrderItem.SalesOrderWhereSalesOrderItem } },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesOrder>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;

                if (@this.CanShip)
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Ship));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Ship));
                }

                if (@this.CanInvoice)
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Invoice));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Invoice));
                }

                if (!@this.SalesOrderInvoiceState.NotInvoiced || !@this.SalesOrderShipmentState.IsNotShipped)
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.SetReadyForPosting));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Post));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Reopen));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Approve));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Hold));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Continue));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Accept));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Revise));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Complete));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Reject));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Cancel));

                    var deniablePermissionByOperandTypeId = new Dictionary<OperandType, Permission>();

                    foreach (Permission permission in @this.Session().Extent<Permission>())
                    {
                        if (permission.ClassPointer == @this.Strategy.Class.Id && permission.Operation == Operations.Write)
                        {
                            deniablePermissionByOperandTypeId.Add(permission.OperandType, permission);
                        }
                    }

                    foreach (var keyValuePair in deniablePermissionByOperandTypeId)
                    {
                        @this.AddDeniedPermission(keyValuePair.Value);
                    }
                }

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
