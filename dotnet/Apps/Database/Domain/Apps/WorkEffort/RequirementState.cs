// <copyright file="WorkEffortState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class RequirementState
    {
        public bool IsCreated => this.Equals(new RequirementStates(this.Strategy.Transaction).Created);

        public bool IsInProgress => this.Equals(new RequirementStates(this.Strategy.Transaction).InProgress);

        public bool IsCancelled => this.Equals(new RequirementStates(this.Strategy.Transaction).Cancelled);

        public bool IsFinished => this.Equals(new RequirementStates(this.Strategy.Transaction).Finished);
    }
}
