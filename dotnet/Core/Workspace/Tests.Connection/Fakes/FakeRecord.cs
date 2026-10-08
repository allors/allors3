// <copyright file="FakeRecord.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Connection
{
    using Allors.Ranges;
    using Allors.Workspace.Connection;
    using Allors.Workspace.Meta;

    /// <summary>
    /// A record with an id and a version and nothing else, for the cache tests.
    /// </summary>
    public sealed class FakeRecord : IRecord
    {
        private static readonly IRanges<long> Ranges = new DefaultStructRanges<long>();

        public FakeRecord(IClass @class, long id, long version)
        {
            this.Class = @class;
            this.Id = id;
            this.Version = version;
        }

        public IClass Class { get; }

        public long Id { get; }

        public long Version { get; }

        public IRange<long> GrantIds => Ranges.Load();

        public IRange<long> RevocationIds => Ranges.Load();

        public object GetRole(IRoleType roleType) => null;

        public bool IsPermitted(long permission) => false;
    }
}
