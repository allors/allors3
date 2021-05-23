// <copyright file="BalanceType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class BalanceType
    {
        public bool IsBalance => this.Equals(new BalanceTypes(this.Strategy.Transaction).Balance);

        public bool IsProfitLoss => this.Equals(new BalanceTypes(this.Strategy.Transaction).ProfitLoss);
    }
}
