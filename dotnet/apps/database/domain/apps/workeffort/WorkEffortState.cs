// <copyright file="WorkEffortState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class WorkEffortState
    {
        public bool IsCreated => this.Equals(new WorkEffortStates(this.Strategy.Transaction).Created);

        public bool IsInProgress => this.Equals(new WorkEffortStates(this.Strategy.Transaction).InProgress);

        public bool IsCompleted => this.Equals(new WorkEffortStates(this.Strategy.Transaction).Completed);

        public bool IsFinished => this.Equals(new WorkEffortStates(this.Strategy.Transaction).Finished);

        public bool IsCancelled => this.Equals(new WorkEffortStates(this.Strategy.Transaction).Cancelled);
    }
}
