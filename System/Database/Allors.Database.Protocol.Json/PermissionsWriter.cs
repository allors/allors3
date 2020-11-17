// <copyright file="PermissionsWriter.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Linq;
    using Allors.Protocol.Json.Api;

    internal class PermissionsWriter
    {
        private readonly IAccessControlLists acls;

        internal PermissionsWriter(IAccessControlLists acls) => this.acls = acls;

        public string Write(IObject @object)
        {
            var deniedPermissionIds = this.acls[@object].DeniedPermissionIds?.OrderBy(v => v);
            var joinedDeniedPermissions = deniedPermissionIds == null ? null : string.Join(Encoding.Separator, deniedPermissionIds);
            return !string.IsNullOrWhiteSpace(joinedDeniedPermissions) ? joinedDeniedPermissions : null;
        }
    }
}
