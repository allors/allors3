// <copyright file="SalesOrderItems.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Allors;

    public partial class SalesOrderItems
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.SalesOrderItemState);

        protected override void AppsSecure(Security config)
        {
            var provisional = new SalesOrderItemStates(this.Transaction).Provisional;
            var readyForPosting = new SalesOrderItemStates(this.Transaction).ReadyForPosting;
            var awaitingAcceptance = new SalesOrderItemStates(this.Transaction).AwaitingAcceptance;
            var inProcess = new SalesOrderItemStates(this.Transaction).InProcess;
            var cancelled = new SalesOrderItemStates(this.Transaction).Cancelled;
            var rejected = new SalesOrderItemStates(this.Transaction).Rejected;
            var completed = new SalesOrderItemStates(this.Transaction).Completed;
            var finished = new SalesOrderItemStates(this.Transaction).Finished;

            var cancel = this.Meta.Cancel;
            var reject = this.Meta.Reject;
            var delete = this.Meta.Delete;
            var reopen = this.Meta.Reopen;
            var approve = this.Meta.Approve;

            config.Deny(this.ObjectType, provisional, reopen, approve);
            config.Deny(this.ObjectType, awaitingAcceptance, reopen, approve, delete);
            config.Deny(this.ObjectType, readyForPosting, reopen, approve);
            config.Deny(this.ObjectType, inProcess, delete, reopen, approve);
            config.Deny(this.ObjectType, completed, delete, cancel, reject, reopen, approve);
            config.Deny(this.ObjectType, cancelled, cancel, reject, approve);
            config.Deny(this.ObjectType, rejected, cancel, reject, approve);

            config.Deny(this.ObjectType, awaitingAcceptance, Operations.Write);
            config.Deny(this.ObjectType, inProcess, Operations.Write);
            config.Deny(this.ObjectType, cancelled, Operations.Write);
            config.Deny(this.ObjectType, rejected, Operations.Write);
            config.Deny(this.ObjectType, completed, Operations.Execute, Operations.Write);
            config.Deny(this.ObjectType, finished, Operations.Execute, Operations.Write);
        }
    }
}
