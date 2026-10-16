// <copyright file="ScopedConnection.InvokeRead.cs" company="Allors bv">
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
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.InvokeRead;
    using Allors.Protocol.Json.Api.PushRead;
    using Allors.Protocol.Json.Api.Read;

    internal sealed partial class ScopedConnection
    {
        private readonly IInvokeReadTransport invokeReadTransport;

        internal async Task<IInvokeResult> InvokeRead(Session session, Invocation[] methods, bool requestedRefresh, CancellationToken cancellationToken)
        {
            this.RequireActive(session);
            var capability = this.invokeReadTransport ?? throw new NotSupportedException("Configure an IInvokeReadTransport on this ready connection before using Invoke.");
            var scope = this.Store?.Scope ?? throw new InvalidOperationException("Await connection creation before Invoke so its read scope is ready.");
            var request = capability.CaptureRequest(new InvokeReadRequest
            { v = 1, i = Guid.NewGuid().ToString("N"), sc = WireScope(scope), x = this.context, l = methods, refresh = requestedRefresh });
            var refresh = new MutationRefresh(request.refresh ? MutationReadStatus.Unavailable : MutationReadStatus.NotRequested);
            var application = MutationRefreshApplicationStatus.NotApplicable;
            var applicationErrors = new List<MutationApplicationError>();
            await this.operations.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                this.RequireActive(session);
                cancellationToken.ThrowIfCancellationRequested();
                if (request.l.Any(method => this.Store.IsUnavailable(method.i)))
                    throw new InvalidOperationException("An Invoke target is unavailable. Pull it successfully before invoking it.");
                // Capture the host-visible request before the handoff; capture failures have sent nothing.
                var outgoing = capability.CaptureRequest(request);
                this.RequireActive(session);
                cancellationToken.ThrowIfCancellationRequested();
                string raw;
                InvokeReadReceipt receipt;
                try
                {
                    raw = await capability.Send(outgoing, cancellationToken).ConfigureAwait(false);
                    receipt = capability.CaptureReceipt(raw, capability.CaptureRequest(request));
                }
                catch (Exception error)
                {
                    session.RequireReconciliation();
                    return new ScopedInvokeResult(session, MutationOutcome.Unknown,
                        new MutationDiagnostics(errors: new[] { new MutationDiagnostic("mutation-unknown", "The dispatched invocation may have committed. Reconcile its outcome in a new Session; do not replay it.", cause: error) }),
                        refresh, new MutationApplication(MutationAcknowledgementStatus.NotApplicable, application), true);
                }
                var outcome = receipt.invoke.k == "committed" ? MutationOutcome.Committed : receipt.invoke.k == "rejected" ? MutationOutcome.Rejected : MutationOutcome.Unknown;
                var errors = (receipt.invoke.e ?? Array.Empty<ReadError>()).Select(error => new MutationDiagnostic(error.c, error.m, error.o, error.i)).ToList();
                if (outcome != MutationOutcome.Committed && errors.Count == 0 && string.IsNullOrWhiteSpace(receipt.invoke._e) &&
                    !(receipt.invoke._d?.Any(error => !string.IsNullOrWhiteSpace(error.m)) ?? false))
                    errors.Add(new MutationDiagnostic(outcome == MutationOutcome.Unknown ? "mutation-unknown" : "invoke-rejected",
                        outcome == MutationOutcome.Unknown ? "The invocation may have committed. Reconcile its outcome before further mutation; do not replay it." : "The invocation was rejected. Inspect its target and derivation diagnostics before invoking again."));
                var diagnostics = ScopedPushResult.CaptureDiagnostics(new InvokeResponse
                { _e = receipt.invoke._e, _v = receipt.invoke._v, _a = receipt.invoke._a, _m = receipt.invoke._m, _d = receipt.invoke._d }, errors);
                if (outcome != MutationOutcome.Committed)
                {
                    if (outcome == MutationOutcome.Unknown) session.RequireReconciliation();
                    return new ScopedInvokeResult(session, outcome, diagnostics, refresh,
                        new MutationApplication(MutationAcknowledgementStatus.NotApplicable, application), session.ReconciliationRequired);
                }
                PushReadChanged changed = null;
                if (request.refresh)
                {
                    try
                    {
                        var part = capability.CapturePart(raw, capability.CaptureRequest(request));
                        // Only a fully validated failure is evidence that the connection scope changed.
                        // A successful subtree must match the request in its strict codec before arriving here.
                        if (part is ReadFailure failure)
                        {
                            this.RequirePartScope(failure);
                            throw new ReadOperationException(failure);
                        }
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
                if (changed != null)
                {
                    var admitted = false;
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        this.RequireActive(session);
                        admitted = true;
                        var completed = this.coordinator.ImportChanged(changed);
                        var result = this.ApplyCompleted(session, completed);
                        refresh = new MutationRefresh(MutationReadStatus.Succeeded, result);
                        application = result.MergeErrors.Any() ? MutationRefreshApplicationStatus.Conflicted : MutationRefreshApplicationStatus.Applied;
                    }
                    catch (Exception error)
                    {
                        application = admitted ? MutationRefreshApplicationStatus.Failed : MutationRefreshApplicationStatus.Skipped;
                        applicationErrors.Add(new MutationApplicationError(MutationApplicationStage.Refresh, error.Message, error));
                    }
                }
                if (application == MutationRefreshApplicationStatus.Applied || application == MutationRefreshApplicationStatus.Conflicted)
                {
                    try { session.NotifyScopedChanges(); }
                    catch (Exception error) { applicationErrors.Add(new MutationApplicationError(MutationApplicationStage.Notification, error.Message, error)); }
                }
                return new ScopedInvokeResult(session, outcome, diagnostics, refresh,
                    new MutationApplication(MutationAcknowledgementStatus.NotApplicable, application, applicationErrors), session.ReconciliationRequired);
            }
            finally { this.operations.Release(); }
        }
    }
}
