// <copyright file="Requirement.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Requirement
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.Requirement, this.M.Requirement.RequirementState),
        };

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistRequirementState)
            {
                this.RequirementState = new RequirementStates(this.Strategy.Session).Active;
            }
        }

        public void AppsClose(RequirementClose method) => this.RequirementState = new RequirementStates(this.Strategy.Session).Closed;

        public void AppsReopen(RequirementReopen method) => this.RequirementState = new RequirementStates(this.Strategy.Session).Active;

        public void AppsCancel(RequirementCancel method) => this.RequirementState = new RequirementStates(this.Strategy.Session).Cancelled;

        public void AppsHold(RequirementHold method) => this.RequirementState = new RequirementStates(this.Strategy.Session).OnHold;
    }
}
