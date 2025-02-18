// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class PurchaseOrderItemStoredInFacilityRule : Rule
    {
        public PurchaseOrderItemStoredInFacilityRule(MetaPopulation m) : base(m, new Guid("3d54205c-b62d-426a-add3-da83f4a63d59")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrder.RolePattern(v => v.StoredInFacility, v => v.PurchaseOrderItems),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrderItem>())
            {
                @this.DerivePurchaseOrderItemStoredInFacility(validation);
            }
        }
    }

    public static class PurchaseOrderItemStoredInFacilityRuleExtensions
    {
        public static void DerivePurchaseOrderItemStoredInFacility(this PurchaseOrderItem @this, IValidation validation)
        {
            if (!@this.ExistOrderShipmentsWhereOrderItem)
            {
                @this.StoredInFacility = @this.PurchaseOrderWherePurchaseOrderItem.StoredInFacility;
            }
        }
    }
}
