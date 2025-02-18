// <copyright file="WorkRequirements.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>


namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Meta;

    public partial class WorkRequirements
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.RequirementState);

        protected override void AppsPrepare(Security security) => security.AddDependency(this.Meta, this.M.Revocation);

        protected override void AppsSecure(Security config)
        {
            var createdState = new RequirementStates(this.Transaction).Created;
            var inProgressState = new RequirementStates(this.Transaction).InProgress;
            var cancelledState = new RequirementStates(this.Transaction).Cancelled;
            var closedState = new RequirementStates(this.Transaction).Finished;

            var start = this.Meta.Start;
            var close = this.Meta.Close;
            var cancel = this.Meta.Cancel;
            var reopen = this.Meta.Reopen;
            var createWorkTask = this.Meta.CreateWorkTask;
            var delete = this.Meta.Delete;

            config.Deny(this.ObjectType, createdState, reopen, close);

            var inProgressExcept = new HashSet<IOperandType>
            {
                this.Meta.Reopen,
                this.Meta.Close,
            };

            config.DenyExcept(this.ObjectType, inProgressState, inProgressExcept, Operations.Execute, Operations.Write);

            var cancelExcept = new HashSet<IOperandType>
            {
                this.Meta.Reopen,
                this.Meta.Delete,
            };

            config.DenyExcept(this.ObjectType, cancelledState, cancelExcept, Operations.Execute, Operations.Write);

            var closeExcept = new HashSet<IOperandType>
            {
                this.Meta.Reopen,
            };

            config.DenyExcept(this.ObjectType, closedState, closeExcept, Operations.Execute, Operations.Write);

            var revocations = new Revocations(this.Transaction);
            var permissions = new Permissions(this.Transaction);

            revocations.WorkRequirementCancelRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Cancel),
            };

            revocations.WorkRequirementDeleteRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Delete),
            };

            revocations.WorkRequirementReopenRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Reopen),
            };
        }
    }
}
