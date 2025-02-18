// <copyright file="BalanceSide.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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
