// <copyright file="OrderAdjustmentExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public static partial class OrderAdjustmentExtensions
    {
        public static void AppsOnPostDerive(this OrderAdjustment @this, ObjectOnPostDerive method)
        {
            var m = @this.Strategy.Transaction.Database.Services().M;
            method.Derivation.Validation.AssertAtLeastOne(@this, m.OrderAdjustment.Amount, m.OrderAdjustment.Percentage);
            method.Derivation.Validation.AssertExistsAtMostOne(@this, m.OrderAdjustment.Amount, m.OrderAdjustment.Percentage);
        }
    }
}
