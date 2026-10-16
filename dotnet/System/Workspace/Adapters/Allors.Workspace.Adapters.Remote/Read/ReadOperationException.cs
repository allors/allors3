// <copyright file="ReadOperationException.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api.Read;

    /// <summary>A correlated failed attempt; no current state has been published.</summary>
    public sealed class ReadOperationException : Exception
    {
        internal ReadOperationException(ReadFailure failure) : base(string.Join(" ", failure.e.Select(v => v.m)))
        {
            this.RequestId = failure.i;
            this.Scope = failure.sc == null ? null : new ReadStoreScope(failure.sc.p, failure.sc.u, failure.sc.w, failure.sc.r, failure.sc.x);
            this.Errors = Array.AsReadOnly(failure.e.Select(v => new ReadOperationError(v)).ToArray());
        }
        public string RequestId { get; }
        public ReadStoreScope Scope { get; }
        public IReadOnlyList<ReadOperationError> Errors { get; }
    }

    public sealed class ReadOperationError
    {
        internal ReadOperationError(ReadError error)
        {
            this.Code = error.c;
            this.Message = error.m;
            this.SelectionIndex = error.i;
            this.ObjectIds = error.o == null ? null : Array.AsReadOnly(error.o.ToArray());
        }
        public string Code { get; }
        public string Message { get; }
        public int? SelectionIndex { get; }
        public IReadOnlyList<long> ObjectIds { get; }
    }
}
