// <copyright file="Session.PushRead.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Allors.Protocol.Json.Api.Push;

    public partial class Session
    {
        private bool saving;
        private string savingOperation;
        private bool reconciliationRequired;
        private int scopedOperations;

        private void RequireEditing()
        {
            var owner = this.DatabaseConnection.ScopedOwner;
            lock (owner.State)
            {
                owner.RequireActive(this);
                if (this.saving) throw new InvalidOperationException(this.savingOperation == "Push" ? "A save is in progress in this Session. Wait for Push to return before editing or starting another operation." : "Invoke is in progress in this Session. Wait for Invoke before editing or starting another operation.");
                if (this.reconciliationRequired) throw new InvalidOperationException("Reconcile the previous mutation in a new Session before further editing. Inspect existing values or Pull without replaying the mutation.");
            }
        }

        internal IDisposable BeginScopedOperation(bool mutation)
        {
            var owner = this.DatabaseConnection.ScopedOwner;
            lock (owner.State)
            {
                owner.RequireActive(this);
                if (this.saving) throw new InvalidOperationException(this.savingOperation == "Push" ? "A save is in progress in this Session. Wait for Push before starting another operation." : "Invoke is in progress in this Session. Wait for Invoke before starting another operation.");
                if (mutation && this.reconciliationRequired) this.RequireEditing();
                ++this.scopedOperations;
                return new ScopedOperation(() => { lock (owner.State) { --this.scopedOperations; } });
            }
        }

        public override bool ReconciliationRequired => this.reconciliationRequired;

        internal bool IsReconciliationRequired => this.ReconciliationRequired;

        internal void RequireReconciliation()
        {
            lock (this.DatabaseConnection.ScopedOwner.State) this.reconciliationRequired = true;
        }

        private IDisposable BeginProtectedMutation(string operation, CancellationToken cancellationToken)
        {
            var owner = this.DatabaseConnection.ScopedOwner;
            lock (owner.State)
            {
                this.RequireEditing();
                cancellationToken.ThrowIfCancellationRequested();
                if (this.scopedOperations != 0) throw new InvalidOperationException($"Wait for this Session's earlier operation to finish before starting {operation}.");
                this.savingOperation = operation;
                this.saving = true;
            }
            return new ScopedOperation(() => { lock (owner.State) { this.saving = false; this.savingOperation = null; } });
        }

        private async Task<IPushResult> PushProtectedAsync(PushOptions options, CancellationToken cancellationToken)
        {
            using var pause = this.BeginProtectedMutation("Push", cancellationToken);
            return await this.DatabaseConnection.ScopedOwner.PushRead(this, options, cancellationToken).ConfigureAwait(false);
        }

        internal PushRequest CapturePushChanges() => new PushRequest
        {
            n = this.PushToDatabaseTracker.Created?.Select(v => ((DatabaseOriginState)v.DatabaseOriginState).PushNew()).ToArray(),
            o = this.PushToDatabaseTracker.Changed?.Select(v => ((DatabaseOriginState)v.Strategy.DatabaseOriginState).PushExisting()).ToArray(),
        };

        internal void AcknowledgePushRead(PushRequest request, PushResponse response)
        {
            var apply = this.StageScopedPush(request, response);
            apply();
            foreach (var mapping in response.n) this.StrategyByWorkspaceId.Remove(mapping.w);
        }

        private sealed class ScopedOperation : IDisposable
        {
            private Action release;
            internal ScopedOperation(Action release) => this.release = release;
            public void Dispose() => Interlocked.Exchange(ref this.release, null)?.Invoke();
        }
    }
}
