// <copyright file="Id.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    public class DatabaseIdentity : Identity
    {
        public long? WorkspaceId { get; set; }

        public long? DatabaseId { get; set; }

        public override long Id => this.DatabaseId ?? this.WorkspaceId ?? 0;
    }
}
