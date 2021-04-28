// <copyright file="BalanceSide.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class BalanceSide
    {
        public bool IsDebit => this.Equals(new BalanceSides(this.Strategy.Transaction).Debit);

        public bool IsCredit => this.Equals(new BalanceSides(this.Strategy.Transaction).Credit);
    }
}
