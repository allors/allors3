// <copyright file="RequestItems.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class RequestItems
    {
        protected override void AppsPrepare(Security security) => security.AddDependency(this.Meta, this.M.Revocation);

        protected override void AppsSecure(Security config)
        {
            var draft = new RequestItemStates(this.Transaction).Draft;
            var cancelled = new RequestItemStates(this.Transaction).Cancelled;
            var rejected = new RequestItemStates(this.Transaction).Rejected;
            var submitted = new RequestItemStates(this.Transaction).Submitted;
            var quoted = new RequestItemStates(this.Transaction).Quoted;

            var cancel = this.Meta.Cancel;
            var hold = this.Meta.Hold;
            var submit = this.Meta.Submit;
            var delete = this.Meta.Delete;

            config.Deny(this.ObjectType, submitted, submit);
            config.Deny(this.ObjectType, cancelled, cancel, submit, hold);
            config.Deny(this.ObjectType, rejected, cancel, submit, hold);
            config.Deny(this.ObjectType, quoted, cancel, submit, hold, delete);

            config.Deny(this.ObjectType, cancelled, Operations.Write);
            config.Deny(this.ObjectType, rejected, Operations.Write);
            config.Deny(this.ObjectType, quoted, Operations.Write);

            var revocations = new Revocations(this.Transaction);
            var permissions = new Permissions(this.Transaction);

            revocations.RequestItemDeleteRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Delete),
            };
        }
    }
}
