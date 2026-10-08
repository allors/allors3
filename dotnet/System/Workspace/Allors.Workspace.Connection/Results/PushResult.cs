// <copyright file="PushResult.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Allors.Protocol.Json.Api.Push;
    using Meta;

    /// <summary>
    /// What a push answered: the errors, and for every new object the database id the server
    /// gave it.
    /// </summary>
    public sealed class PushResult : Result
    {
        private readonly PushResponse response;

        private IReadOnlyDictionary<long, long> databaseIdByWorkspaceId;

        internal PushResult(IMetaPopulation metaPopulation, PushResponse response) : base(metaPopulation, response) => this.response = response;

        public IReadOnlyDictionary<long, long> DatabaseIdByWorkspaceId =>
            this.databaseIdByWorkspaceId ??= new ReadOnlyDictionary<long, long>(this.response.n?.ToDictionary(v => v.w, v => v.d) ?? new Dictionary<long, long>());
    }
}
