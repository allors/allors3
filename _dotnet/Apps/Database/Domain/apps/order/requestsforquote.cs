// <copyright file="RequestsForQuote.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class RequestsForQuote
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.RequestState);

        protected override void AppsSecure(Security config)
        {
            var anonymous = new RequestStates(this.Transaction).Anonymous;
            var cancelled = new RequestStates(this.Transaction).Cancelled;
            var quoted = new RequestStates(this.Transaction).Quoted;
            var pendingCustomer = new RequestStates(this.Transaction).PendingCustomer;
            var rejected = new RequestStates(this.Transaction).Rejected;
            var submitted = new RequestStates(this.Transaction).Submitted;

            var cancel = this.Meta.Cancel;
            var hold = this.Meta.Hold;
            var submit = this.Meta.Submit;
            var reject = this.Meta.Reject;
            var createQuote = this.Meta.CreateQuote;
            var delete = this.Meta.Delete;

            config.Deny(this.ObjectType, quoted, cancel, hold, submit, reject, createQuote, delete);
            config.Deny(this.ObjectType, submitted, submit);
            config.Deny(this.ObjectType, anonymous, hold, createQuote);
            config.Deny(this.ObjectType, pendingCustomer, hold, createQuote, delete);
            config.Deny(this.ObjectType, rejected, reject, cancel, submit, hold, createQuote);
            config.Deny(this.ObjectType, cancelled, reject, cancel, submit, hold, createQuote);

            config.Deny(this.ObjectType, cancelled, Operations.Write);
            config.Deny(this.ObjectType, rejected, Operations.Write);
            config.Deny(this.ObjectType, quoted, Operations.Write);
        }
    }
}
