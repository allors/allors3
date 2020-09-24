// <copyright file="OperatingBudget.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class OperatingBudget
    {
        public TransitionalConfiguration[] TransitionalConfigurations => new[]{
            new TransitionalConfiguration(this.M.OperatingBudget, this.M.OperatingBudget.BudgetState),
        };
    }
}
