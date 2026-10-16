// <copyright file="ScopedPullResult.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api.Pull;

    /// <summary>Session editing objects with copied read outcomes; it does not retain a read snapshot.</summary>
    public sealed class ScopedPullResult : PullResult
    {
        internal ScopedPullResult(Session session, ReadCompletedResult completed) : base(session, new PullResponse
        {
            o = completed.Objects.ToDictionary(v => v.Key, v => v.Value ?? 0),
            c = completed.Collections.ToDictionary(v => v.Key, v => v.Value.ToArray()),
        })
        {
            this.RequestId = completed.RequestId;
            this.Scope = completed.Scope;
            this.Inputs = Array.AsReadOnly(completed.Inputs.ToArray());
            this.ReadErrors = Array.Empty<ReadOperationError>();
        }

        internal ScopedPullResult(Session session, ReadOperationException failure) : base(session, new PullResponse
        {
            _e = failure.Message,
            o = new Dictionary<string, long>(), c = new Dictionary<string, long[]>(),
        })
        {
            this.RequestId = failure.RequestId;
            this.Scope = failure.Scope;
            this.Inputs = Array.Empty<ReadCompletedInput>();
            this.ReadErrors = failure.Errors;
        }

        public string RequestId { get; }
        public ReadStoreScope Scope { get; }
        public IReadOnlyList<ReadCompletedInput> Inputs { get; }
        public IReadOnlyList<ReadOperationError> ReadErrors { get; }

        internal void Materialize()
        {
            _ = this.Objects;
            _ = this.Collections;
        }
    }
}
