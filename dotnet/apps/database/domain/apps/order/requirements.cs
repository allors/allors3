// <copyright file="Requirements.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Requirements
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.RequirementState);

        protected override void AppsSecure(Security config)
        {
            var createdState = new WorkEffortStates(this.Transaction).Created;
            var cancelledState = new WorkEffortStates(this.Transaction).Cancelled;
            var finishedState = new WorkEffortStates(this.Transaction).Completed;

            config.Deny(this.ObjectType, createdState, this.M.WorkEffort.Reopen);

            config.Deny(this.ObjectType, cancelledState, Operations.Execute, Operations.Write);
            config.Deny(this.ObjectType, finishedState, Operations.Execute, Operations.Read);
        }
    }
}
