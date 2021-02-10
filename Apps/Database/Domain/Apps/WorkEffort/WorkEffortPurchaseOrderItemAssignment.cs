// <copyright file="WorkEffortPurchaseOrderItemAssignment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public partial class WorkEffortPurchaseOrderItemAssignment
    {
        public void AppsDelegateAccess(DelegatedAccessControlledObjectDelegateAccess method)
        {
            if (method.SecurityTokens == null)
            {
                method.SecurityTokens = this.Assignment?.SecurityTokens.ToArray();
            }

            if (method.DeniedPermissions == null)
            {
                method.DeniedPermissions = this.Assignment?.DeniedPermissions.ToArray();
            }
        }

        public void AppsCalculateSellingPrice(WorkEffortPurchaseOrderItemAssignmentCalculateSellingPrice method)
        {
            if (!method.Result.HasValue)
            {
                if (this.AssignedUnitSellingPrice.HasValue)
                {
                    this.UnitSellingPrice = this.AssignedUnitSellingPrice.Value;
                }
                else
                {
                    var part = this.PurchaseOrderItem.Part;

                    var currentPriceComponents = this.Assignment?.TakenBy?.PriceComponentsWherePricedBy
                        .Where(v => v.FromDate <= this.Assignment.ScheduledStart && (!v.ExistThroughDate || v.ThroughDate >= this.Assignment.ScheduledStart))
                        .ToArray();

                    if (currentPriceComponents != null)
                    {
                        var currentPartPriceComponents = part.GetPriceComponents(currentPriceComponents);

                        var price = currentPartPriceComponents.OfType<BasePrice>().Max(v => v.Price);
                        this.UnitSellingPrice = price ?? 0M;
                    }
                    else
                    {
                        this.UnitSellingPrice = 0M;
                    }
                }

                method.Result = true;
            }
        }
    }
}
