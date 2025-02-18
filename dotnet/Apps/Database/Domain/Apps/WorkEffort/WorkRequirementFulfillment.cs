// <copyright file="WorkRequirementFulfillment.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class WorkRequirementFulfillment
    {
        public void AppsDelete(DeletableDelete method)
        {
            if (this.FullfilledBy.RequirementState.IsInProgress)
            {
                this.FullfilledBy.RequirementState = new RequirementStates(this.Strategy.Transaction).Created;
            }
        }
    }
}
