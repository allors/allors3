// <copyright file="DatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Direct.Api
{
    using System;
    using System.Collections.Generic;
    using Database;

    public class Object
    {
        internal Object(IStrategy strategy, long[] accessControls, long[] deniedPermissions)
        {
            this.Class = strategy.Class.Id;
            this.Id = strategy.ObjectId;
            this.Version = strategy.ObjectVersion;
            this.AccessControls = accessControls;
            this.DeniedPermissions = deniedPermissions;
        }

        internal Object(Guid @class, long id, long version, IReadOnlyDictionary<Guid, object> roleByRelationTypeId, long[] accessControls, long[] deniedPermissions)
        {
            this.Class = @class;
            this.Id = id;
            this.Version = version;
            this.RoleByRelationTypeId = roleByRelationTypeId;
            this.AccessControls = accessControls;
            this.DeniedPermissions = deniedPermissions;
        }

        public Guid Class { get; }

        public long Id { get; }

        public long Version { get; }

        public IReadOnlyDictionary<Guid, object> RoleByRelationTypeId { get; }

        public long[] AccessControls { get; }

        public long[] DeniedPermissions { get; }
    }
}
