// <copyright file="ScopedPushResult.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Allors.Protocol.Json.Api;
    using Allors.Protocol.Json.Api.Read;

    /// <summary>An owned save outcome; read and local application failures cannot erase commitment.</summary>
    internal sealed class ScopedPushResult : IPushResult
    {
        internal ScopedPushResult(Session session, MutationOutcome outcome, MutationDiagnostics diagnostics,
            IEnumerable<CreationMapping> mappings, MutationRefresh refresh, MutationApplication application,
            PushDiscovery discovery, IPendingPull pendingQuery, bool reconciliationRequired)
        {
            this.Outcome = outcome;
            this.Diagnostics = diagnostics;
            this.CreationMappings = Array.AsReadOnly(mappings.ToArray());
            this.Refresh = refresh;
            this.Application = application;
            this.Discovery = discovery;
            this.PendingQuery = pendingQuery;
            this.ReconciliationRequired = reconciliationRequired;
            // Only already held objects participate in convenience views. Raw diagnostics remain complete.
            IObject Held(long id) => session.GetStrategy(id)?.Object;
            this.VersionErrors = Array.AsReadOnly(diagnostics.VersionErrorIds.Select(Held).Where(v => v != null).ToArray());
            this.AccessErrors = Array.AsReadOnly(diagnostics.AccessErrorIds.Select(Held).Where(v => v != null).ToArray());
            this.MissingErrors = Array.AsReadOnly(diagnostics.MissingErrorIds.Select(Held).Where(v => v != null).ToArray());
            var relations = session.Workspace.Configuration.MetaPopulation.RelationTypes.ToDictionary(v => v.Tag);
            this.DerivationErrors = Array.AsReadOnly(diagnostics.DerivationErrors.Select(error => (IDerivationError)new HeldDerivationError(
                error.Message, error.Relations.Select(relation =>
                {
                    var obj = Held(relation.ObjectId);
                    return obj != null && relations.TryGetValue(relation.RelationTypeTag, out var type)
                        ? (Role?)new Role(obj, type) : null;
                }).Where(v => v.HasValue).Select(v => v.Value))).ToArray());
        }

        public MutationOutcome Outcome { get; }
        public MutationDiagnostics Diagnostics { get; }
        public MutationRefresh Refresh { get; }
        public MutationApplication Application { get; }
        public bool ReconciliationRequired { get; }
        public IReadOnlyList<CreationMapping> CreationMappings { get; }
        public PushDiscovery Discovery { get; }
        public IPendingPull PendingQuery { get; }
        public bool HasErrors => this.Outcome != MutationOutcome.Committed ||
            !string.IsNullOrEmpty(this.Diagnostics.ErrorMessage) || this.Diagnostics.Errors.Count != 0 ||
            this.Diagnostics.VersionErrorIds.Count != 0 || this.Diagnostics.AccessErrorIds.Count != 0 ||
            this.Diagnostics.MissingErrorIds.Count != 0 || this.Diagnostics.DerivationErrors.Count != 0 ||
            this.Refresh.Status == MutationReadStatus.Failed || this.Refresh.Status == MutationReadStatus.Unavailable ||
            this.Discovery.Status == MutationReadStatus.Failed || this.Discovery.Status == MutationReadStatus.Unavailable ||
            this.Application.Errors.Count != 0 || this.Application.Refresh == MutationRefreshApplicationStatus.Conflicted;
        public string ErrorMessage => string.Join(" ", new[] { this.Diagnostics.ErrorMessage }
            .Concat(this.Diagnostics.Errors.Select(value => value.Message))
            .Concat(this.Diagnostics.DerivationErrors.Select(value => value.Message))
            .Concat(this.Refresh.Errors.Select(value => value.Message))
            .Concat(this.Discovery.Errors.Select(value => value.Message))
            .Concat(this.Application.Errors.Select(value => value.Message))
            .Where(value => !string.IsNullOrWhiteSpace(value)));
        public IEnumerable<IObject> VersionErrors { get; }
        public IEnumerable<IObject> AccessErrors { get; }
        public IEnumerable<IObject> MissingErrors { get; }
        public IEnumerable<IDerivationError> DerivationErrors { get; }

        internal static MutationDiagnostics CaptureDiagnostics(Response response, IEnumerable<MutationDiagnostic> errors = null) =>
            new MutationDiagnostics(response._e, errors, response._v, response._a, response._m,
                response._d?.Where(v => v != null).Select(v => new MutationDerivationDiagnostic(v.m,
                    v.r?.Where(r => r != null).Select(r => new MutationDerivationRelation(r.i, r.r)))));

        internal static MutationDiagnostic[] ReadErrors(Exception error, string code) => error is ReadOperationException failure
            ? failure.Errors.Select(v => new MutationDiagnostic(v.Code, v.Message, v.ObjectIds, v.SelectionIndex, error)).ToArray()
            : new[] { new MutationDiagnostic(code, string.IsNullOrWhiteSpace(error.Message) ? "Read the affected objects or query again before relying on their current state." : error.Message, cause: error) };

        private sealed class HeldDerivationError : IDerivationError
        {
            internal HeldDerivationError(string message, IEnumerable<Role> roles)
            { this.Message = message; this.Roles = Array.AsReadOnly(roles.ToArray()); }
            public string Message { get; }
            public IEnumerable<Role> Roles { get; }
        }
    }

    internal sealed class PendingPull : IPendingPull
    {
        private Func<CancellationToken, Task<IPullResult>> complete;
        internal PendingPull(Func<CancellationToken, Task<IPullResult>> complete) => this.complete = complete;
        public Task<IPullResult> CompleteAsync(CancellationToken cancellationToken = default)
        {
            var operation = Interlocked.Exchange(ref this.complete, null);
            return operation == null
                ? Task.FromException<IPullResult>(new InvalidOperationException("This pending query was already used. Start a new Pull to refresh the query again."))
                : operation(cancellationToken);
        }
    }
}
