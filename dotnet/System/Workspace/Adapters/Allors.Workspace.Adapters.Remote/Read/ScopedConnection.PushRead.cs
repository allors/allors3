// <copyright file="ScopedConnection.PushRead.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.PushRead;
    using Allors.Protocol.Json.Api.Read;
    using Data;

    internal sealed partial class ScopedConnection
    {
        private readonly IPushReadTransport pushReadTransport;

        private ReadMetadata pushMetadata;

        internal void ConfigurePushMetadata(Meta.IMetaPopulation configuration) => this.pushMetadata = new ReadMetadata(configuration);

        internal async Task<IPushResult> PushRead(Session session, PushOptions options, CancellationToken cancellationToken)
        {
            this.RequireActive(session);
            var capability = this.pushReadTransport ?? throw new NotSupportedException("Configure an IPushReadTransport on this ready connection before using Push.");
            var scope = this.Store?.Scope ?? throw new InvalidOperationException("Await connection creation before Push so its read scope is ready.");
            var changes = session.CapturePushChanges();
            var request = capability.CaptureRequest(new PushReadRequest
            {
                v = 1, i = Guid.NewGuid().ToString("N"), sc = WireScope(scope), x = this.context,
                push = new PushReadChanges { n = changes.n, o = changes.o }, returnChanged = options.Refresh,
                discover = options.Discover == null ? null : new ReadPull
                {
                    l = options.Discover.Select(pull =>
                    {
                        if (pull == null) throw new ArgumentException("Supply non-null declarative Pull inputs.", nameof(options));
                        var visitor = new ReadToJsonVisitor(this.connection.UnitConvert, committedArguments: true);
                        pull.Accept(visitor);
                        return visitor.Pull;
                    }).ToArray(),
                    d = Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>(),
                },
            });
            var refresh = new MutationRefresh(request.returnChanged == true ? MutationReadStatus.Unavailable : MutationReadStatus.NotRequested);
            var discovery = new PushDiscovery(request.discover != null ? MutationReadStatus.Unavailable : MutationReadStatus.NotRequested);
            var acknowledgement = MutationAcknowledgementStatus.NotApplicable;
            var refreshApplication = MutationRefreshApplicationStatus.NotApplicable;
            var applicationErrors = new List<MutationApplicationError>();
            await this.operations.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                this.RequireActive(session);
                cancellationToken.ThrowIfCancellationRequested();
                if ((request.push.o ?? Array.Empty<PushRequestObject>()).Any(v => this.Store.IsUnavailable(v.d)))
                    throw new InvalidOperationException("A mutation target is unavailable. Pull it successfully before Push.");
                // Capture the host-visible request before the handoff; capture failures have sent nothing.
                var outgoing = capability.CaptureRequest(request);
                this.RequireActive(session);
                cancellationToken.ThrowIfCancellationRequested();
                string raw;
                PushReadReceipt receipt;
                try
                {
                    raw = await capability.Send(outgoing, cancellationToken).ConfigureAwait(false);
                    receipt = capability.CaptureReceipt(raw, capability.CaptureRequest(request));
                }
                catch (Exception error)
                {
                    session.RequireReconciliation();
                    return new ScopedPushResult(session, MutationOutcome.Unknown,
                        new MutationDiagnostics(errors: new[] { new MutationDiagnostic("mutation-unknown", "The dispatched save may have committed. Reconcile its outcome before further mutation; do not replay it.", cause: error) }),
                        Array.Empty<CreationMapping>(), refresh, new MutationApplication(acknowledgement, refreshApplication), discovery, null, true);
                }

                var outcome = receipt.push.k == "committed" ? MutationOutcome.Committed : receipt.push.k == "rejected" ? MutationOutcome.Rejected : MutationOutcome.Unknown;
                var errors = (receipt.push.e ?? Array.Empty<ReadError>()).Select(v => new MutationDiagnostic(v.c, v.m, v.o, v.i)).ToList();
                if (outcome != MutationOutcome.Committed && errors.Count == 0 && string.IsNullOrWhiteSpace(receipt.push._e) &&
                    !(receipt.push._d?.Any(v => !string.IsNullOrWhiteSpace(v.m)) ?? false))
                    errors.Add(outcome == MutationOutcome.Unknown
                        ? new MutationDiagnostic("mutation-unknown", "The dispatched save may have committed. Reconcile its outcome before further mutation; do not replay it.")
                        : new MutationDiagnostic("push-rejected", "The save was rejected. Inspect its object and derivation diagnostics, correct the changes, and save explicitly."));
                var diagnostics = ScopedPushResult.CaptureDiagnostics(new PushResponse
                { _e = receipt.push._e, _v = receipt.push._v, _a = receipt.push._a, _m = receipt.push._m, _d = receipt.push._d }, errors);
                if (outcome != MutationOutcome.Committed)
                {
                    if (outcome == MutationOutcome.Unknown) session.RequireReconciliation();
                    return new ScopedPushResult(session, outcome, diagnostics, Array.Empty<CreationMapping>(), refresh,
                        new MutationApplication(acknowledgement, refreshApplication), discovery, null, session.ReconciliationRequired);
                }
                var mappings = receipt.push.n.Select(v => new CreationMapping(v.w, v.d)).ToArray();
                try
                {
                    lock (this.State)
                    {
                        this.RequireActive(session);
                        foreach (var mapping in mappings)
                        {
                            if (this.Store.GetCurrentRecord(mapping.DatabaseId) != null || this.Store.IsUnavailable(mapping.DatabaseId))
                                throw new FormatException("Push mapped a new object to an identity already known by this connection. Reconcile the committed save before further editing.");
                        }
                        session.AcknowledgePushRead(new PushRequest { n = request.push.n, o = request.push.o }, new PushResponse { n = receipt.push.n });
                        acknowledgement = MutationAcknowledgementStatus.Applied;
                    }
                }
                catch (Exception error)
                {
                    session.RequireReconciliation();
                    acknowledgement = MutationAcknowledgementStatus.Failed;
                    applicationErrors.Add(new MutationApplicationError(MutationApplicationStage.Acknowledgement, error.Message, error));
                }

                // Classify detached read evidence before applying lifetime/cancellation gates. ImportChanged
                // publishes into the cache and checks lifetime, so it cannot perform this independent classification.
                PushReadChanged changed = null;
                DiscoverySuccess discovered = null;
                if (request.returnChanged == true)
                {
                    try
                    {
                        var part = capability.CapturePart(raw, capability.CaptureRequest(request), true);
                        this.RequirePartScope(part);
                        if (part is ReadFailure failure) throw new ReadOperationException(failure);
                        changed = (PushReadChanged)part;
                        this.ValidateChangedMetadata(session, changed);
                        refresh = new MutationRefresh(MutationReadStatus.Succeeded);
                    }
                    catch (Exception error)
                    {
                        changed = null;
                        refresh = new MutationRefresh(MutationReadStatus.Failed, errors: ScopedPushResult.ReadErrors(error, "refresh-failed"));
                    }
                }
                if (request.discover != null)
                {
                    try
                    {
                        var part = capability.CapturePart(raw, capability.CaptureRequest(request), false);
                        this.RequirePartScope(part);
                        if (part is ReadFailure failure) throw new ReadOperationException(failure);
                        discovered = (DiscoverySuccess)part;
                        foreach (var header in discovered.p) this.pushMetadata.Class(header.c);
                        discovery = new PushDiscovery(MutationReadStatus.Succeeded);
                    }
                    catch (Exception error)
                    {
                        discovered = null;
                        discovery = new PushDiscovery(MutationReadStatus.Failed, ScopedPushResult.ReadErrors(error, "discovery-failed"));
                    }
                }

                if (changed != null)
                {
                    var admitted = false;
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        this.RequireActive(session);
                        if (acknowledgement != MutationAcknowledgementStatus.Applied)
                            throw new InvalidOperationException("Reconcile the committed acknowledgement before accepting its refreshed records.");
                        admitted = true;
                        var completed = this.coordinator.ImportChanged(changed);
                        var result = this.ApplyCompleted(session, completed);
                        refresh = new MutationRefresh(MutationReadStatus.Succeeded, result);
                        refreshApplication = result.MergeErrors.Any() ? MutationRefreshApplicationStatus.Conflicted : MutationRefreshApplicationStatus.Applied;
                    }
                    catch (Exception error)
                    {
                        refreshApplication = admitted ? MutationRefreshApplicationStatus.Failed : MutationRefreshApplicationStatus.Skipped;
                        applicationErrors.Add(new MutationApplicationError(MutationApplicationStage.Refresh, error.Message, error));
                    }
                }
                IPendingPull pending = null;
                if (discovered != null)
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        this.RequireActive(session);
                        if (acknowledgement != MutationAcknowledgementStatus.Applied)
                            throw new InvalidOperationException("Reconcile the committed acknowledgement before completing its query.");
                        var query = this.transport.CaptureRequest(new ReadRequest
                        { v = 1, i = request.i, k = "read", sc = request.sc, x = this.context, p = request.discover });
                        pending = new PendingPull(token => this.CompletePending(session, query, discovered, token));
                    }
                    catch (Exception error) { applicationErrors.Add(new MutationApplicationError(MutationApplicationStage.Discovery, error.Message, error)); }
                }
                try { session.NotifyScopedChanges(); }
                catch (Exception error) { applicationErrors.Add(new MutationApplicationError(MutationApplicationStage.Notification, error.Message, error)); }
                return new ScopedPushResult(session, outcome, diagnostics, mappings, refresh,
                    new MutationApplication(acknowledgement, refreshApplication, applicationErrors), discovery, pending, session.ReconciliationRequired);
            }
            finally { this.operations.Release(); }
        }

        private void ValidateChangedMetadata(Session session, PushReadChanged changed)
        {
            // The read codec owns scalar normalization for both serializers without publishing records.
            var request = new ReadRequest
            {
                v = 1, i = changed.i, k = "read", sc = changed.sc, x = this.context,
                p = new ReadPull { l = Array.Empty<Allors.Protocol.Json.Data.Pull>(), d = Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>() },
            };
            var complete = (ReadSuccess)this.transport.CaptureResponse(new ReadSuccess
            {
                i = changed.i, k = "complete", sc = changed.sc, p = changed.p, s = changed.s, pm = changed.pm,
                l = Array.Empty<ReadInput>(), c = new Dictionary<string, long[]>(), o = new Dictionary<string, long?>(),
            }, request);
            var scope = new ReadStoreScope(complete.sc.p, complete.sc.u, complete.sc.w, complete.sc.r, complete.sc.x);
            ReadSecurityKey Key(ReadSecurityHeader header) => new ReadSecurityKey(header.k, header.i, header.v);
            foreach (var permission in complete.pm)
                this.pushMetadata.Permission(new ReadStoredPermission(scope, permission.i, permission.p.c, (Operations)permission.p.o.Value, permission.p.t));
            var records = complete.p.Select(record =>
            {
                var existing = session.GetStrategy(record.i);
                if (existing != null && existing.Class.Tag != record.c)
                    throw new FormatException("A refreshed record changed a known object's class. Read that object successfully before further editing.");
                var value = new ReadStoredRecord(scope, record.i, record.c, record.v,
                    record.g.Select(index => Key(complete.s[index])), record.r.Select(index => Key(complete.s[index])), this.pushMetadata.Roles(record.c, record.p.ro));
                this.pushMetadata.Record(value);
                return value;
            }).ToArray();
            this.pushMetadata.Pool(records);
        }

        private ScopedPullResult ApplyCompleted(Session session, ReadCompletedResult completed)
        {
            try { return this.ApplyCompletedCore(session, completed); }
            catch
            {
                session.RequireReconciliation();
                session.BlockScoped();
                throw;
            }
        }

        private ScopedPullResult ApplyCompletedCore(Session session, ReadCompletedResult completed)
        {
            var records = completed.Records.Select(v => DatabaseRecord.FromScoped(this.connection, v, this.classes[v.ClassTag], this.relations)).ToArray();
            foreach (var record in records)
            {
                var existing = session.GetStrategy(record.Id);
                if (existing != null && !ReferenceEquals(existing.Class, record.Class))
                    throw new FormatException("A refreshed record changed an existing object's class. Reconcile the committed mutation before further editing.");
            }
            var staged = session.StageScoped(records);
            var result = new ScopedPullResult(session, completed);
            _ = completed.Objects.ToDictionary(v => v.Key.ToUpperInvariant());
            _ = completed.Collections.ToDictionary(v => v.Key.ToUpperInvariant());
            lock (this.State)
            {
                this.RequireActive(session);
                session.ApplyScoped(staged, result);
                result.Materialize();
            }
            return result;
        }

        private async Task<IPullResult> CompletePending(Session session, ReadRequest query, DiscoverySuccess discovery, CancellationToken cancellationToken)
        {
            using var operation = session.BeginScopedOperation(false);
            await this.operations.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                this.RequireActive(session);
                ReadCompletedResult completed;
                try { completed = await this.coordinator.CompleteAfterDiscovery(query, discovery, cancellationToken).ConfigureAwait(false); }
                catch (ReadOperationException failure) { this.RequireActive(session); return new ScopedPullResult(session, failure); }
                {
                    this.RequireActive(session);
                    var result = this.ApplyCompleted(session, completed);
                    session.NotifyScopedChanges();
                    return result;
                }
            }
            finally { this.operations.Release(); }
        }

        private void RequirePartScope(object part)
        {
            var scope = part is PushReadChanged changed ? changed.sc : part is DiscoverySuccess discovery ? discovery.sc : (part as ReadFailure)?.sc;
            if (scope == null) return;
            if (!this.Store.Scope.Equals(new ReadStoreScope(scope.p, scope.u, scope.w, scope.r, scope.x)))
            {
                this.Retire();
                if (part is ReadFailure failure) throw new ReadOperationException(failure);
                throw new InvalidOperationException("The read outcome has a different scope. Create a new ready connection; the committed receipt is preserved.");
            }
        }

        private static ReadScope WireScope(ReadStoreScope scope) => new ReadScope { p = scope.Population, u = scope.UserId, w = scope.Workspace, r = scope.Revision, x = scope.Context };
    }
}
