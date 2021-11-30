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
                this.RequirementState = new RequirementStates(this.Strategy.Transaction).Created;
            }
        }

        public void AppsCreateWorkTask(RequirementCreateWorkTask method)
        {
            this.RequirementState = new RequirementStates(this.Strategy.Transaction).Closed;
            method.StopPropagation = true;
        }

        public void AppsCancel(RequirementCancel method)
        {
            this.RequirementState = new RequirementStates(this.Strategy.Transaction).Cancelled;
            method.StopPropagation = true;
        }
    }
}
