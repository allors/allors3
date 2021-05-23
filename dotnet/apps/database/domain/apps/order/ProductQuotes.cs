// <copyright file="ProductQuotes.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Meta;

    public partial class ProductQuotes
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.QuoteState);

        protected override void AppsSecure(Security config)
        {
            var created = new QuoteStates(this.Transaction).Created;
            var awaitingApproval = new QuoteStates(this.Transaction).AwaitingApproval;
            var inProcess = new QuoteStates(this.Transaction).InProcess;
            var awaitingAcceptance = new QuoteStates(this.Transaction).AwaitingAcceptance;
            var accepted = new QuoteStates(this.Transaction).Accepted;
            var ordered = new QuoteStates(this.Transaction).Ordered;
            var rejected = new QuoteStates(this.Transaction).Rejected;
            var cancelled = new QuoteStates(this.Transaction).Cancelled;

            var setReadyForProcessing = this.Meta.SetReadyForProcessing;
            var approve = this.Meta.Approve;
            var send = this.Meta.Send;
            var accept = this.Meta.Accept;
            var revise = this.Meta.Revise;
            var reopen = this.Meta.Reopen;
            var reject = this.Meta.Reject;
            var order = this.Meta.Order;
            var cancel = this.Meta.Cancel;
            var delete = this.Meta.Delete;

            config.Deny(this.ObjectType, created, approve, order, reopen, send, accept, revise);
            config.Deny(this.ObjectType, awaitingApproval, setReadyForProcessing, send, accept, reopen, order, cancel, revise);
            config.Deny(this.ObjectType, inProcess, setReadyForProcessing, approve, accept, order, reopen, delete);
            config.Deny(this.ObjectType, awaitingAcceptance, setReadyForProcessing, approve, order, send, reopen, reject, delete);
            config.Deny(this.ObjectType, accepted, setReadyForProcessing, approve, send, accept, reject, reopen, delete);
            config.Deny(this.ObjectType, ordered, setReadyForProcessing, approve, reject, order, cancel, reopen, send, accept, revise, delete);
            config.Deny(this.ObjectType, rejected, setReadyForProcessing, approve, reject, order, send, accept, cancel, revise);
            config.Deny(this.ObjectType, cancelled, setReadyForProcessing, cancel, reject, order, send, accept, approve, revise);

            var except = new HashSet<IOperandType>
            {
                this.Meta.ElectronicDocuments,
                this.Meta.Print,
            };

            config.DenyExcept(this.ObjectType, inProcess, except, Operations.Write);
            config.DenyExcept(this.ObjectType, rejected, except, Operations.Write);
            config.DenyExcept(this.ObjectType, awaitingAcceptance, except, Operations.Write);
            config.DenyExcept(this.ObjectType, accepted, except, Operations.Write);
            config.DenyExcept(this.ObjectType, ordered, except, Operations.Write);
            config.DenyExcept(this.ObjectType, cancelled, except, Operations.Write);
        }
    }
}
