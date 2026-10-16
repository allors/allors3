// <copyright file="ReadCoordinator.cs" company="Allors bv">
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
    using Allors.Protocol.Json.Api.Read;
    using Allors.Protocol.Json.Api.PushRead;
    using Meta;

    /// <summary>
    /// Connection-owned reader: discover, then complete the original query with immutable cache entries.
    /// Its caller holds the connection operation gate; this helper owns no queue or lifetime.
    /// </summary>
    internal sealed class ReadCoordinator
    {
        private readonly ScopedConnection owner;
        private readonly IReadTransport transport;
        private readonly ReadMetadata metadata;
        private readonly string workspace;
        private readonly string revision;
        private readonly string context;
        private readonly int maxHints;
        private readonly int maxRequestBytes;

        internal ReadCoordinator(ScopedConnection owner, IReadTransport transport, IMetaPopulation metadata, string workspace, string revision,
            string context = null, int maxHints = 256, int maxRequestBytes = 1024 * 1024)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this.metadata = new ReadMetadata(metadata);
            this.workspace = !string.IsNullOrEmpty(workspace) ? workspace : throw new ArgumentException("Configure the admitted workspace name.", nameof(workspace));
            this.revision = !string.IsNullOrEmpty(revision) ? revision : throw new ArgumentException("Configure the matching representation revision.", nameof(revision));
            this.context = context;
            this.maxHints = maxHints >= 0 && maxHints <= 70000 ? maxHints : throw new ArgumentOutOfRangeException(nameof(maxHints), "Choose a cache inventory bound from zero through 70000 rows.");
            this.maxRequestBytes = maxRequestBytes > 0 && maxRequestBytes <= 1024 * 1024 ? maxRequestBytes : throw new ArgumentOutOfRangeException(nameof(maxRequestBytes), "Choose a positive request byte bound no larger than 1 MiB.");
        }

        internal ReadStore Store => this.owner.Store;

        internal ReadRequest Capture(ReadRequest request)
        {
            var captured = this.transport.CaptureRequest(request);
            if (captured.k != "read" || captured.x != this.context ||
                (captured.h != null && (captured.h.r.Length != 0 || captured.h.s.Length != 0 || captured.h.p.Length != 0)))
            {
                throw new ArgumentException("Supply a read request with the configured exact context and no caller cache hints. The connection owns discovery and cache inventories.", nameof(request));
            }
            captured.h = null;
            if (captured.sc != null) this.ValidateScope(Scope(captured.sc), this.Store?.Scope);
            this.RequireRequestSize(captured);
            return captured;
        }

        internal Task<ReadCompletedResult> Read(ReadRequest request, CancellationToken cancellationToken = default)
        {
            this.Check(cancellationToken);
            return this.ReadCore(this.Capture(request), cancellationToken);
        }

        internal Task<ReadCompletedResult> CompleteAfterDiscovery(ReadRequest request, DiscoverySuccess discovery, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var captured = this.transport.CaptureRequest(request);
            lock (this.owner.State)
            {
                this.owner.RequireReaderActive();
                this.ValidateScope(Scope(captured.sc), this.Store?.Scope);
                this.ValidateScope(Scope(discovery.sc), this.Store?.Scope);
            }
            return this.ReadCore(captured, cancellationToken, discovery);
        }

        internal ReadCompletedResult ImportChanged(PushReadChanged changed)
        {
            var request = new ReadRequest { v = 1, i = changed.i, k = "read", sc = changed.sc, x = this.context,
                p = new ReadPull { l = Array.Empty<Allors.Protocol.Json.Data.Pull>(), d = Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>() } };
            var complete = (ReadSuccess)this.transport.CaptureResponse(new ReadSuccess
            {
                i = changed.i, k = "complete", sc = changed.sc, p = changed.p, s = changed.s, pm = changed.pm,
                l = Array.Empty<ReadInput>(), c = new Dictionary<string, long[]>(), o = new Dictionary<string, long?>(),
            }, request);
            var scope = Scope(complete.sc);
            this.ValidateScope(scope, this.Store?.Scope);
            var security = complete.s.Select(v => new ReadStoredSecurity(scope, Key(v), v.p.p)).ToArray();
            var permissions = complete.pm.Select(v => new ReadStoredPermission(scope, v.i, v.p.c, (Operations)v.p.o.Value, v.p.t)).ToArray();
            foreach (var permission in permissions) this.metadata.Permission(permission);
            var records = complete.p.Select(v => new ReadStoredRecord(scope, v.i, v.c, v.v,
                v.g.Select(i => Key(complete.s[i])), v.r.Select(i => Key(complete.s[i])), this.metadata.Roles(v.c, v.p.ro))).ToArray();
            foreach (var record in records) this.metadata.Record(record);
            this.metadata.Pool(records);
            lock (this.owner.State)
            {
                this.owner.RequireReaderActive();
                if (this.owner.Store == null) throw new InvalidOperationException("Bind a scoped read before applying PushRead.");
                var snapshot = this.owner.Store.ImportAndPublish(records, security, permissions, changed.u);
                return new ReadCompletedResult(complete, snapshot);
            }
        }

        private static ReadStoreScope Scope(ReadScope scope) => new ReadStoreScope(scope.p, scope.u, scope.w, scope.r, scope.x);
        private static ReadScope WireScope(ReadStoreScope scope) => new ReadScope { p = scope.Population, u = scope.UserId, w = scope.Workspace, r = scope.Revision, x = scope.Context };
        private static ReadSecurityKey Key(ReadSecurityHeader header) => new ReadSecurityKey(header.k, header.i, header.v);

        private async Task<ReadCompletedResult> ReadCore(ReadRequest request, CancellationToken cancellationToken, DiscoverySuccess suppliedDiscovery = null)
        {
            this.Check(cancellationToken);
            var activeStore = this.Store;
            var expectedScope = activeStore?.Scope ?? (request.sc == null ? null : Scope(request.sc));
            if (request.sc != null) { this.ValidateScope(Scope(request.sc), activeStore?.Scope); }
            request.sc = expectedScope == null ? null : WireScope(expectedScope);
            var discoveryRequest = this.transport.CaptureRequest(request);
            discoveryRequest.k = "discover";
            var discovery = suppliedDiscovery ?? await this.Send(discoveryRequest, expectedScope, cancellationToken).ConfigureAwait(false) as DiscoverySuccess
                ?? throw new FormatException("The discovery attempt must return discovery headers before a complete read.");
            expectedScope = Scope(discovery.sc);
            foreach (var header in discovery.p) { this.metadata.Class(header.c); }
            request.sc = WireScope(expectedScope);

            var inventory = this.Hints(request, activeStore, discovery);
            this.Check(cancellationToken);
            var complete = await this.Send(request, expectedScope, cancellationToken).ConfigureAwait(false) as ReadSuccess
                ?? throw new FormatException("The complete read attempt must return a complete result.");
            var scope = Scope(complete.sc);
            var security = complete.s.Select(v => v.p.r.HasValue
                ? inventory.Security[v.p.r.Value]
                : new ReadStoredSecurity(scope, Key(v), v.p.p)).ToArray();
            var permissions = complete.pm.Select(v => v.p.r.HasValue
                ? inventory.Permissions[v.p.r.Value]
                : new ReadStoredPermission(scope, v.i, v.p.c, (Operations)v.p.o.Value, v.p.t)).ToArray();
            foreach (var permission in permissions) { this.metadata.Permission(permission); }
            var records = complete.p.Select(v => v.p.r.HasValue
                ? inventory.Records[v.p.r.Value]
                : new ReadStoredRecord(scope, v.i, v.c, v.v, v.g.Select(i => Key(complete.s[i])), v.r.Select(i => Key(complete.s[i])), this.metadata.Roles(v.c, v.p.ro))).ToArray();
            foreach (var record in records) { this.metadata.Record(record); }
            this.metadata.Pool(records);
            var unavailable = complete.l.Where(v => v.s == "unavailable" && v.o.HasValue).Select(v => v.o.Value).ToArray();

            this.Check(cancellationToken);
            lock (this.owner.State)
            {
                this.owner.RequireReaderActive();
                cancellationToken.ThrowIfCancellationRequested();
                activeStore = this.owner.Store ?? new ReadStore(scope);
                this.ValidateScope(scope, activeStore.Scope);
                var snapshot = activeStore.ImportAndPublish(records, security, permissions, unavailable, cancellationToken);
                this.owner.Store = activeStore;
                return new ReadCompletedResult(complete, snapshot);
            }
        }

        private async Task<object> Send(ReadRequest request, ReadStoreScope expectedScope, CancellationToken cancellationToken)
        {
            this.RequireRequestSize(request);
            var original = this.transport.CaptureRequest(request);
            var outgoing = this.transport.CaptureRequest(request);
            this.Check(cancellationToken);
            var response = await this.transport.Read(outgoing, cancellationToken).ConfigureAwait(false);
            this.Check(cancellationToken);
            var captured = this.transport.CaptureResponse(response, original);
            var responseScope = captured is ReadSuccess success ? success.sc : captured is DiscoverySuccess discovery ? discovery.sc : (captured as ReadFailure)?.sc;
            if (responseScope != null)
            {
                try { this.ValidateScope(Scope(responseScope), expectedScope); }
                catch (Exception exception) when (exception is FormatException || exception is ArgumentException)
                {
                    this.owner.Retire();
                    if (captured is ReadFailure scopedFailure) { throw new ReadOperationException(scopedFailure); }
                    throw new InvalidOperationException("The server returned a different read scope. Establish the new identity and metadata, then create a new connection.", exception);
                }
            }

            if (captured is ReadFailure failure) { throw new ReadOperationException(failure); }
            this.Check(cancellationToken);
            return captured;
        }

        private Inventory Hints(ReadRequest request, ReadStore activeStore, DiscoverySuccess discovery)
        {
            if (activeStore == null || this.maxHints == 0) { this.RequireRequestSize(request); return new Inventory(); }
            var records = new List<ReadStoredRecord>();
            foreach (var selection in request.p.l.Where(v => v.o.HasValue))
            {
                var record = activeStore.GetCurrentRecord(selection.o.Value);
                if (record != null) { records.Add(record); }
            }

            foreach (var header in discovery.p)
            {
                var record = activeStore.GetRecord(header.i, header.c, header.v, header.g.Select(i => Key(discovery.s[i])), header.r.Select(i => Key(discovery.s[i])));
                if (record != null) { records.Add(record); }
            }

            var selected = new List<ReadStoredRecord>();
            var keys = new List<ReadSecurityKey>();
            foreach (var record in records.Distinct())
            {
                var extra = record.Grants.Concat(record.Revocations).Where(v => !keys.Contains(v)).ToArray();
                if (selected.Count + keys.Count + 1 + extra.Length > this.maxHints) { continue; }
                selected.Add(record);
                keys.AddRange(extra);
            }

            var bodies = keys.Select(activeStore.GetSecurity).ToArray();
            var permissions = bodies.Where(v => v != null).SelectMany(v => v.PermissionIds)
                .Distinct().Select(activeStore.GetPermission).Where(v => v != null).Take(this.maxHints - selected.Count - keys.Count).ToArray();
            var cache = new ReadCache
            {
                r = selected.Select(v => new ReadRecordHeader { i = v.Id, c = v.ClassTag, v = v.DataVersion,
                    g = v.Grants.Select(key => keys.IndexOf(key)).ToArray(), r = v.Revocations.Select(key => keys.IndexOf(key)).ToArray() }).ToArray(),
                s = keys.Select((v, i) => new ReadSecurityHint { k = v.Kind, i = v.Id, v = v.Version, h = bodies[i] != null }).ToArray(),
                p = permissions.Select(v => v.Id).ToArray(),
            };
            request.h = cache;
            if (this.transport.RequestByteCount(request) > this.maxRequestBytes)
            {
                // Cache hints are optional. Dropping the inventory preserves the original declaration.
                request.h = null;
                this.RequireRequestSize(request);
                return new Inventory();
            }

            return new Inventory(selected.ToArray(), bodies, permissions);
        }

        private void ValidateScope(ReadStoreScope scope, ReadStoreScope expected)
        {
            if (scope.Workspace != this.workspace || scope.Revision != this.revision || scope.Context != this.context || (expected != null && !scope.Equals(expected)))
            { throw new FormatException("Use the exact configured workspace, representation, context, population and user scope."); }
        }

        private void RequireRequestSize(ReadRequest request)
        {
            if (this.transport.RequestByteCount(request) > this.maxRequestBytes)
            { throw new FormatException("Reduce the original read selection to the configured request byte bound; cache hints cannot make this declaration fit."); }
        }

        private void Check(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (this.owner.State) this.owner.RequireReaderActive();
        }

        private sealed class Inventory
        {
            internal Inventory(ReadStoredRecord[] records = null, ReadStoredSecurity[] security = null, ReadStoredPermission[] permissions = null)
            {
                this.Records = records ?? Array.Empty<ReadStoredRecord>();
                this.Security = security ?? Array.Empty<ReadStoredSecurity>();
                this.Permissions = permissions ?? Array.Empty<ReadStoredPermission>();
            }
            internal ReadStoredRecord[] Records { get; }
            internal ReadStoredSecurity[] Security { get; }
            internal ReadStoredPermission[] Permissions { get; }
        }
    }
}
