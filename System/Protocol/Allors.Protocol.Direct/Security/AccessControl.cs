// <copyright file="Session.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Direct.Api
{
    public class AccessControl
    {
        public AccessControl(long id, long version)
        {
            this.Id = id;
            this.Version = version;
        }

        public AccessControl(long id, long version, long[] permissions)
        {
            this.Id = id;
            this.Version = version;
            this.Permissions = permissions;
        }

        public long Id { get; }

        public long Version { get; }

        public long[] Permissions { get; }
    }
}
