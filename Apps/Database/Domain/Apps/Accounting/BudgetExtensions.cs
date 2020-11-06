// <copyright file="BudgetExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public static partial class BudgetExtensions
    {
        public static void AppsClose(this Budget @this, BudgetClose method) => @this.BudgetState = new BudgetStates(@this.Strategy.Session).Closed;

        public static void AppsReopen(this Budget @this, BudgetReopen method) => @this.BudgetState = new BudgetStates(@this.Strategy.Session).Opened;
    }
}
