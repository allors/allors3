// <copyright file="ReadCompletedResult.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Allors.Protocol.Json.Api.Read;

    /// <summary>An immutable complete read whose values remain inspectable independently of cache lookup.</summary>
    internal sealed class ReadCompletedResult
    {
        internal ReadCompletedResult(ReadSuccess response, ReadSnapshot snapshot)
        {
            this.Scope = snapshot.Scope;
            this.RequestId = response.i;
            this.Records = snapshot.Records;
            this.Security = snapshot.Security;
            this.Permissions = snapshot.Permissions;
            this.Inputs = Array.AsReadOnly(response.l.Select(v => new ReadCompletedInput(v.i, v.o, v.s)).ToArray());
            this.Collections = new ReadOnlyDictionary<string, IReadOnlyList<long>>(response.c.ToDictionary(v => v.Key, v => (IReadOnlyList<long>)Array.AsReadOnly(v.Value.ToArray())));
            this.Objects = new ReadOnlyDictionary<string, long?>(new Dictionary<string, long?>(response.o));
        }

        public ReadStoreScope Scope { get; }
        public string RequestId { get; }
        public IReadOnlyList<ReadStoredRecord> Records { get; }
        public IReadOnlyList<ReadStoredSecurity> Security { get; }
        public IReadOnlyList<ReadStoredPermission> Permissions { get; }
        public IReadOnlyList<ReadCompletedInput> Inputs { get; }
        public IReadOnlyDictionary<string, IReadOnlyList<long>> Collections { get; }
        public IReadOnlyDictionary<string, long?> Objects { get; }
    }

    public sealed class ReadCompletedInput
    {
        internal ReadCompletedInput(int index, long? objectId, string status)
        { this.Index = index; this.ObjectId = objectId; this.Status = status; }
        public int Index { get; }
        public long? ObjectId { get; }
        public string Status { get; }
    }
}
