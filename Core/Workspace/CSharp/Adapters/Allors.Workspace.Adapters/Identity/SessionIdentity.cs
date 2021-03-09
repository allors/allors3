// <copyright file="Id.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    public class SessionIdentity : Identity
    {
        public SessionIdentity(long id) => this.Id = id;

        public override long Id { get; }

        public override long CompareId => this.Id;
    }
}
