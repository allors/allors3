// <copyright file="Requirement.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class WorkRequirement
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.WorkRequirement, this.M.WorkRequirement.RequirementState),
        };

        public void AppsCreateWorkTask(WorkRequirementCreateWorkTask method)
        {
            this.RequirementState = new RequirementStates(this.Strategy.Transaction).Closed;
            method.StopPropagation = true;
        }
    }
}
