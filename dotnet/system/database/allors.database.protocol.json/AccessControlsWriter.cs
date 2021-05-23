// <copyright file="AccessControlsWriter.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Collections.Generic;
    using System.Linq;
    using Security;

    internal class AccessControlsWriter
    {
        private readonly IAccessControlLists acls;

        internal AccessControlsWriter(IAccessControlLists acls) => this.acls = acls;

        public IEnumerable<long> Write(IObject @object) => this.acls[@object].AccessControls?.Select(v => v.Strategy.ObjectId).Distinct();
    }
}
