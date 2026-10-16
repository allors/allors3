// <copyright file="ScopedConnection.cs" company="Allors bv">
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
    using Data;
    using Meta;

    internal sealed partial class ScopedConnection
    {
        private readonly DatabaseConnection connection;
        private readonly IReadTransport transport;
        private readonly ReadCoordinator coordinator;
        private readonly SemaphoreSlim operations = new SemaphoreSlim(1, 1);
        private readonly string context;
        private readonly IReadOnlyDictionary<string, IClass> classes;
        private readonly IReadOnlyDictionary<string, IRelationType> relations;

        internal ScopedConnection(DatabaseConnection connection, IReadTransport transport, string workspace, string revision, string context, IPushReadTransport pushReadTransport = null, IInvokeReadTransport invokeReadTransport = null)
        {
            this.connection = connection;
            this.transport = transport;
            this.context = context;
            this.pushReadTransport = pushReadTransport;
            this.invokeReadTransport = invokeReadTransport;
            var metadata = connection.Configuration.MetaPopulation;
            this.classes = metadata.Classes.ToDictionary(v => v.Tag);
            this.relations = metadata.RelationTypes.ToDictionary(v => v.Tag);
            this.coordinator = new ReadCoordinator(this, transport, metadata, workspace, revision, context);
        }

        internal object State => this.connection.State;
        internal string Context => this.context;
        internal bool IsRetired => this.connection.IsClosing;
        internal ReadStore Store { get; set; }

        internal async Task InitializeAsync(CancellationToken cancellationToken)
        {
            var request = this.transport.CaptureRequest(new ReadRequest
            {
                v = 1, i = Guid.NewGuid().ToString("N"), k = "read", x = this.context,
                p = new ReadPull { l = Array.Empty<Allors.Protocol.Json.Data.Pull>(), d = Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>() },
            });
            await this.Read(request, cancellationToken).ConfigureAwait(false);
        }

        internal void RequireReaderActive() => this.connection.RequireOperational();

        internal async Task<ReadCompletedResult> Read(ReadRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var captured = this.coordinator.Capture(request);
            await this.operations.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                this.RequireReaderActive();
                return await this.coordinator.Read(captured, cancellationToken).ConfigureAwait(false);
            }
            finally { this.operations.Release(); }
        }

        internal void RequireActive(Session session)
        {
            lock (this.State)
            {
                if (this.IsRetired)
                { throw new InvalidOperationException("This connection is retired. Create a new connection and Session for the current identity and configuration."); }
                this.connection.RequireReady();
                session.RequireUsable();
            }
        }

        internal void Retire() => _ = this.connection.DisposeAsync();

        internal async Task DrainAsync()
        {
            await this.operations.WaitAsync().ConfigureAwait(false);
            this.operations.Release();
        }

        internal void Clear() => this.Store?.Clear();

        internal async Task<IPullResult> Pull(Session session, Pull[] pulls, CancellationToken cancellationToken)
        {
            using var operation = session.BeginScopedOperation(false);
            this.RequireActive(session);
            var request = new ReadRequest
            {
                v = 1, i = Guid.NewGuid().ToString("N"), k = "read", x = this.context,
                p = new ReadPull
                {
                    l = pulls.Select(v =>
                    {
                        if (v == null) { throw new ArgumentException("Supply non-null declarative Pull inputs.", nameof(pulls)); }
                        var visitor = new ReadToJsonVisitor(this.connection.UnitConvert);
                        v.Accept(visitor);
                        return visitor.Pull;
                    }).ToArray(),
                    d = Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>(),
                },
            };
            // Strict codec capture occurs before the outer queue, including declarations and arguments.
            return await this.PullQueued(session, this.transport.CaptureRequest(request), cancellationToken).ConfigureAwait(false);
        }

        internal bool IsPermitted(Session session, long id, IClass type, IOperandType operand, Operations operation, bool initialAllowed)
        {
            lock (this.State)
            {
                this.RequireActive(session);
                if (id > 0 && this.Store?.IsUnavailable(id) == true) { return false; }
                if (initialAllowed) { return true; }
                if (id <= 0 || this.Store == null) { return false; }
                var tag = operand is IRoleType role ? role.RelationType.Tag : ((IMethodType)operand).Tag;
                return this.Store.IsPermitted(id, type.Tag, operation, tag);
            }
        }

        private async Task<IPullResult> PullQueued(Session session, ReadRequest request, CancellationToken cancellationToken)
        {
            await this.operations.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                this.RequireActive(session);
                ReadCompletedResult completed;
                try { completed = await this.coordinator.Read(request, cancellationToken: cancellationToken).ConfigureAwait(false); }
                catch (ReadOperationException failure)
                {
                    try { this.RequireActive(session); }
                    catch (InvalidOperationException retired)
                    { throw new InvalidOperationException(retired.Message, failure); }
                    return new ScopedPullResult(session, failure);
                }
                this.RequireActive(session);
                try
                {
                    var records = completed.Records.Select(v => DatabaseRecord.FromScoped(this.connection, v, this.classes[v.ClassTag], this.relations)).ToArray();
                    var staged = session.StageScoped(records);
                    var result = new ScopedPullResult(session, completed);
                    // Detect duplicate case-insensitive output labels before any Session state changes.
                    _ = completed.Objects.ToDictionary(v => v.Key.ToUpperInvariant());
                    _ = completed.Collections.ToDictionary(v => v.Key.ToUpperInvariant());
                    lock (this.State)
                    {
                        this.RequireActive(session);
                        session.ApplyScoped(staged, result);
                        result.Materialize();
                    }
                    session.NotifyScopedChanges();
                    return result;
                }
                catch
                {
                    session.BlockScoped();
                    throw;
                }
            }
            finally { this.operations.Release(); }
        }
    }
}
