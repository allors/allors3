// <copyright file="PriceableExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    internal static class PriceableExtensions
    {
        public static decimal SetUnitSurcharge(this Priceable @this, PriceComponent priceComponent, decimal revenueBreakSurcharge)
        {
            var m = @this.DatabaseState().M;
            if (priceComponent.Strategy.Class.Equals(m.SurchargeComponent.ObjectType))
            {
                var surchargeComponent = (SurchargeComponent)priceComponent;
                decimal surcharge;

                if (surchargeComponent.Price.HasValue)
                {
                    surcharge = surchargeComponent.Price.Value;
                    @this.UnitSurcharge += surcharge;
                }
                else
                {
                    var percentage = surchargeComponent.Percentage ?? 0;
                    surcharge = Math.Round(@this.UnitBasePrice * percentage / 100, 2);
                    @this.UnitSurcharge += surcharge;
                }
            }

            return revenueBreakSurcharge;
        }

        internal static decimal SetUnitDiscount(this Priceable @this, PriceComponent priceComponent, decimal revenueBreakDiscount)
        {
            var m = @this.DatabaseState().M;
            if (priceComponent.Strategy.Class.Equals(m.DiscountComponent.ObjectType))
            {
                var discountComponent = (DiscountComponent)priceComponent;
                decimal discount;

                if (discountComponent.Price.HasValue)
                {
                    discount = discountComponent.Price.Value;
                    @this.UnitDiscount += discount;
                }
                else
                {
                    var percentage = discountComponent.Percentage ?? 0;
                    discount = Math.Round(@this.UnitBasePrice * percentage / 100, 2);
                    @this.UnitDiscount += discount;
                }
            }

            return revenueBreakDiscount;
        }
    }
}
