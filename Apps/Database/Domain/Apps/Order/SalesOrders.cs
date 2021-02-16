// <copyright file="SalesOrders.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Meta;

    public partial class SalesOrders
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.SalesOrderState);

        protected override void AppsSecure(Security config)
        {
            var provisional = new SalesOrderStates(this.Transaction).Provisional;
            var onHold = new SalesOrderStates(this.Transaction).OnHold;
            var requestsApproval = new SalesOrderStates(this.Transaction).RequestsApproval;
            var readyForPosting = new SalesOrderStates(this.Transaction).ReadyForPosting;
            var awaitingAcceptance = new SalesOrderStates(this.Transaction).AwaitingAcceptance;
            var inProcess = new SalesOrderStates(this.Transaction).InProcess;
            var cancelled = new SalesOrderStates(this.Transaction).Cancelled;
            var rejected = new SalesOrderStates(this.Transaction).Rejected;
            var completed = new SalesOrderStates(this.Transaction).Completed;
            var finished = new SalesOrderStates(this.Transaction).Finished;
            var tranferred = new SalesOrderStates(this.Transaction).Transferred;

            var setReadyForPosting = this.Meta.SetReadyForPosting;
            var post = this.Meta.Post;
            var reopen = this.Meta.Reopen;
            var approve = this.Meta.Approve;
            var hold = this.Meta.Hold;
            var @continue = this.Meta.Continue;
            var accept = this.Meta.Accept;
            var revise = this.Meta.Revise;
            var complete = this.Meta.Complete;
            var ship = this.Meta.Ship;
            var invoice = this.Meta.Invoice;
            var reject = this.Meta.Reject;
            var cancel = this.Meta.Cancel;
            var transfer = this.Meta.DoTransfer;

            config.Deny(this.ObjectType, provisional, reject, approve, @continue, ship, invoice, post, accept, reopen, revise);
            config.Deny(this.ObjectType, requestsApproval, setReadyForPosting, hold, @continue, ship, invoice, post, accept, reopen, revise, transfer);
            config.Deny(this.ObjectType, readyForPosting, setReadyForPosting, approve, complete, @continue, ship, invoice, accept, reopen, transfer);
            config.Deny(this.ObjectType, awaitingAcceptance, setReadyForPosting, post, approve, hold, @continue, complete, ship, invoice, reopen, transfer);
            config.Deny(this.ObjectType, inProcess, setReadyForPosting, post, accept, reject, approve, @continue, reopen, transfer);
            config.Deny(this.ObjectType, onHold, setReadyForPosting, reject, approve, hold, ship, invoice, post, accept, revise, transfer);
            config.Deny(this.ObjectType, rejected, reject, ship, invoice, post, accept, hold, @continue, revise, approve, setReadyForPosting, cancel, transfer);
            config.Deny(this.ObjectType, cancelled, cancel, ship, invoice, post, accept, hold, @continue, revise, approve, setReadyForPosting, reject, transfer);
            //config.Deny(this.ObjectType, tranferred, cancel, ship, invoice, post, accept, hold, @continue, revise, approve, setReadyForPosting, reject, transfer);
            config.Deny(this.ObjectType, completed, complete, reject, cancel, approve, hold, @continue, setReadyForPosting, invoice, post, accept, reopen, revise, transfer);

            var except = new HashSet<OperandType>
            {
                this.Meta.ElectronicDocuments,
                this.Meta.Print,
            };

            config.DenyExcept(this.ObjectType, readyForPosting, except, Operations.Write);
            config.DenyExcept(this.ObjectType, awaitingAcceptance, except, Operations.Write);
            config.DenyExcept(this.ObjectType, inProcess, except, Operations.Write);
            config.DenyExcept(this.ObjectType, cancelled, except, Operations.Write);
            config.DenyExcept(this.ObjectType, rejected, except, Operations.Write);
            config.DenyExcept(this.ObjectType, completed, except, Operations.Write);
            config.DenyExcept(this.ObjectType, finished, except, Operations.Execute, Operations.Write);
        }
    }
}
