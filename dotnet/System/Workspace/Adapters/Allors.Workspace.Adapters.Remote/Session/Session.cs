// <copyright file="RemoteSession.cs" company="Allors bv">
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
    using Allors.Protocol.Json.Api;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Data;
    using Meta;
    using Protocol.Json;
    using InvokeOptions = Allors.Workspace.InvokeOptions;

    public partial class Session : Adapters.Session, IDisposable
    {
        internal Session(Workspace workspace, ISessionServices sessionServices) : base(workspace, sessionServices)
        {
        }

        private DatabaseConnection DatabaseConnection => this.Workspace.DatabaseConnection;

        private bool closed;
        private bool inconsistent;
        private bool notifyScoped;

        public bool IsClosed => this.closed;

        protected override void AssertActive() => this.DatabaseConnection.ScopedOwner.RequireActive(this);

        protected override void AssertEditable() => this.RequireEditing();

        public override void Dispose()
        {
            var owner = this.DatabaseConnection.ScopedOwner;
            lock (owner.State)
            {
                if (this.closed) { return; }
                this.closed = true;
            }
        }

        internal void RequireUsable()
        {
            this.Workspace.RequireUsable();
            if (this.closed || this.inconsistent)
            { throw new InvalidOperationException("This Session is closed or could not complete adaptation. Create a new Session; existing diffs remain available for inspection."); }
        }

        internal void BlockScoped() { lock (this.DatabaseConnection.ScopedOwner.State) { this.inconsistent = true; } }

        internal void NotifyScopedChanges()
        {
            if (this.notifyScoped) { this.notifyScoped = false; this.OnChanged(EventArgs.Empty); }
        }

        internal ScopedReadStage StageScoped(DatabaseRecord[] records)
        {
            var stages = records.Select(record =>
            {
                var existing = this.GetStrategy(record.Id);
                var strategy = existing ?? new Strategy(this, record);
                _ = strategy.Object; // Object factories may invoke host code; run them before final admission.
                return new ScopedStage { Record = record, Strategy = strategy, IsNew = existing == null,
                    Accept = existing == null || ((DatabaseOriginState)strategy.DatabaseOriginState).CanAcceptScoped(record) };
            }).ToArray();
            // Tracker surfaces allow caller-supplied sets. Enumerate them during staging and only
            // mutate these built-in copies in the final callback-free application section.
            return new ScopedReadStage
            {
                Records = stages,
                Instantiated = new HashSet<IStrategy>(this.ChangeSetTracker.Instantiated ?? Enumerable.Empty<IStrategy>()),
                DatabaseOriginStates = new HashSet<Adapters.DatabaseOriginState>(this.ChangeSetTracker.DatabaseOriginStates ?? Enumerable.Empty<Adapters.DatabaseOriginState>()),
            };
        }

        internal void ApplyScoped(ScopedReadStage staged, ScopedPullResult result)
        {
            foreach (var stage in staged.Records)
            {
                var state = (DatabaseOriginState)stage.Strategy.DatabaseOriginState;
                if (!stage.Accept) { result.AddMergeError(stage.Strategy.Object); continue; }
                if (stage.IsNew)
                {
                    this.AddStrategy(stage.Strategy);
                    staged.Instantiated.Add(stage.Strategy);
                }
                else { state.AcceptScoped(stage.Record); }
                staged.DatabaseOriginStates.Add(state);
                this.notifyScoped = true;
            }
            this.ChangeSetTracker.Instantiated = staged.Instantiated;
            this.ChangeSetTracker.DatabaseOriginStates = staged.DatabaseOriginStates;
        }

        internal sealed class ScopedReadStage
        {
            internal ScopedStage[] Records;
            internal HashSet<IStrategy> Instantiated;
            internal HashSet<Adapters.DatabaseOriginState> DatabaseOriginStates;
        }

        internal sealed class ScopedStage
        {
            internal DatabaseRecord Record;
            internal Adapters.Strategy Strategy;
            internal bool IsNew;
            internal bool Accept;
        }

        public new Workspace Workspace => (Workspace)base.Workspace;

        public override T Create<T>(IClass @class)
        {
            this.AssertEditable();
            var workspaceId = base.Workspace.DatabaseConnection.NextId();
            var strategy = new Strategy(this, @class, workspaceId);
            this.AddStrategy(strategy);
            this.PushToDatabaseTracker.OnCreated(strategy);
            this.ChangeSetTracker.OnCreated(strategy);
            return (T)strategy.Object;
        }

        public override Task<IInvokeResult> InvokeAsync(Method method, InvokeOptions options = null) => this.InvokeAsync(new[] { method }, options, CancellationToken.None);

        public override Task<IInvokeResult> InvokeAsync(Method[] methods, InvokeOptions options = null) => this.InvokeAsync(methods, options, CancellationToken.None);

        public override Task<IInvokeResult> InvokeAsync(Method method, InvokeOptions options, CancellationToken cancellationToken) => this.InvokeAsync(new[] { method }, options, cancellationToken);

        public override async Task<IInvokeResult> InvokeAsync(Method[] methods, InvokeOptions options, CancellationToken cancellationToken)
        {
            this.AssertActive();
            using var pause = this.BeginProtectedMutation("Invoke", cancellationToken);
            if (methods == null) throw new ArgumentException("Supply a non-null method array; use an empty array for an atomic no-op.", nameof(methods));
            var metadata = this.Workspace.Configuration.MetaPopulation;
            var captured = methods.Select(method =>
            {
                if (method.Object?.Strategy is not Strategy strategy || !ReferenceEquals(strategy.Session, this))
                    throw new ArgumentException("Invoke methods on objects accepted by this Session. Pull the target into this Session before invoking it.", nameof(methods));
                if (strategy.Id <= 0 || strategy.DatabaseOriginState.Version <= 0)
                    throw new ArgumentException("Pull a committed target with a positive baseline version before invoking its method.", nameof(methods));
                if (method.MethodType == null || !metadata.MethodTypes.Any(v => ReferenceEquals(v, method.MethodType)) ||
                    !strategy.Class.MethodTypes.Any(v => ReferenceEquals(v, method.MethodType)))
                    throw new ArgumentException("Use a method declared or inherited by the target's captured metadata class.", nameof(methods));
                return new Invocation { i = strategy.Id, v = strategy.DatabaseOriginState.Version, m = method.MethodType.Tag };
            }).ToArray();
            return await this.DatabaseConnection.ScopedOwner.InvokeRead(this, captured, options?.Refresh ?? true, cancellationToken).ConfigureAwait(false);
        }

        public override Task<IPullResult> PullAsync(params Pull[] pulls) => this.DatabaseConnection.ScopedOwner.Pull(this, pulls, CancellationToken.None);

        public Task<IPullResult> PullAsync(CancellationToken cancellationToken, params Pull[] pulls) => this.DatabaseConnection.ScopedOwner.Pull(this, pulls, cancellationToken);

        public override Task<IPushResult> PushAsync() => this.PushAsync(null, CancellationToken.None);

        public Task<IPushResult> PushAsync(CancellationToken cancellationToken) => this.PushAsync(null, cancellationToken);

        public override Task<IPushResult> PushAsync(PushOptions options, CancellationToken cancellationToken = default) =>
            this.PushProtectedAsync(options ?? new PushOptions(), cancellationToken);

        private Action StageScopedPush(PushRequest request, PushResponse response)
        {
            var staged = this.ValidateCreationMappings((request.n ?? Array.Empty<PushRequestNewObject>()).Select(v => v.w).ToArray(), response);
            var changed = (request.o ?? Array.Empty<PushRequestObject>()).Select(v =>
                this.GetStrategy(v.d) ?? throw new InvalidOperationException("A submitted object left this Session before its Push response. Reconcile this mutation.")).ToArray();
            return () =>
            {
                // Every identity is validated before any changes; these sealed strategies invoke no host tracker callbacks.
                foreach (var mapping in staged)
                {
                    mapping.Key.OnDatabasePushNewId(mapping.Value);
                    this.AddStrategy(mapping.Key);
                    mapping.Key.OnDatabasePushed();
                }
                this.PushToDatabaseTracker.Created = null;
                this.PushToDatabaseTracker.Changed = null;
                foreach (var strategy in changed) strategy.OnDatabasePushed();
            };
        }

        private List<KeyValuePair<Adapters.Strategy, long>> ValidateCreationMappings(long[] expectedIds, PushResponse response)
        {
            var expected = new HashSet<long>(expectedIds);
            var mappings = response?.n?.Select(v => v == null ? null : new PushResponseNewObject { w = v.w, d = v.d }).ToArray()
                ?? Array.Empty<PushResponseNewObject>();
            const string message = "The save may have committed, but its creation mappings are incomplete, repeated, or collide with a held object. Reconcile the created objects before further mutation; do not replay this save.";
            if (response == null || expected.Count != expectedIds.Length || mappings.Length != expected.Count) throw new FormatException(message);
            var workspaceIds = new HashSet<long>();
            var databaseIds = new HashSet<long>();
            var staged = new List<KeyValuePair<Adapters.Strategy, long>>();
            foreach (var mapping in mappings)
            {
                if (mapping == null || mapping.w >= 0 || mapping.d <= 0 || !expected.Contains(mapping.w) ||
                    !workspaceIds.Add(mapping.w) || !databaseIds.Add(mapping.d) || this.GetStrategy(mapping.d) != null)
                    throw new FormatException(message);
                var strategy = this.GetStrategy(mapping.w);
                if (strategy == null || strategy.Id != mapping.w) throw new FormatException(message);
                staged.Add(new KeyValuePair<Adapters.Strategy, long>(strategy, mapping.d));
            }
            return staged;
        }

    }
}
