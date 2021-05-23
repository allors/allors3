// <copyright file="PermissionsWriter.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Collections.Generic;
    using System.Linq;
    using Security;

    internal class PermissionsWriter
    {
        private readonly IAccessControlLists acls;

        internal PermissionsWriter(IAccessControlLists acls) => this.acls = acls;

        public IEnumerable<long> Write(IObject @object) => this.acls[@object].DeniedPermissionIds?.Distinct();
    }
}
