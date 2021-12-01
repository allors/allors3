// <copyright file="WorkRequirements.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>


namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Meta;

    public partial class WorkRequirements
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.RequirementState);

        protected override void AppsSecure(Security config)
        {
            var createdState = new RequirementStates(this.Transaction).Created;
            var activeState = new RequirementStates(this.Transaction).Active;
            var cancelledState = new RequirementStates(this.Transaction).Cancelled;
            var closedState = new RequirementStates(this.Transaction).Closed;

            var cancel = this.Meta.Cancel;
            var reopen = this.Meta.Reopen;
            var createWorkTask = this.Meta.CreateWorkTask;
            var delete = this.Meta.Delete;

            config.Deny(this.ObjectType, createdState, reopen);
            config.Deny(this.ObjectType, activeState, cancel, delete, createWorkTask);
            config.Deny(this.ObjectType, closedState, Operations.Execute, Operations.Write);

            var except = new HashSet<IOperandType>
            {
                this.Meta.Reopen,
            };

            config.DenyExcept(this.ObjectType, cancelledState, except, Operations.Write);
        }
    }
}
