// <copyright file="LocalAccessControl.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;

    internal class LocalAccessControl
    {
        internal LocalAccessControl(long id) => this.Id = id;

        internal long Id { get; }

        internal long Version { get; set; }

        internal ISet<long> PermissionIds { get; set; }
    }
}
