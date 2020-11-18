// <copyright file="AccessControlsWriter.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Linq;
    using Allors.Protocol.Json.Api;
    using Security;

    internal class AccessControlsWriter
    {
        private readonly IAccessControlLists acls;

        internal AccessControlsWriter(IAccessControlLists acls) => this.acls = acls;

        public string Write(IObject @object)
        {
            var accessControls = this.acls[@object].AccessControls?.Select(v => v.Strategy.ObjectId).OrderBy(v => v);
            var joinedAccessControls = accessControls != null ? string.Join(Encoding.Separator, accessControls) : null;
            return !string.IsNullOrWhiteSpace(joinedAccessControls) ? joinedAccessControls : null;
        }
    }
}
