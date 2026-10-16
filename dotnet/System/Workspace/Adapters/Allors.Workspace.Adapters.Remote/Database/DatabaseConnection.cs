// <copyright file="RemoteDatabase.cs" company="Allors bv">
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
    using Allors.Protocol.Json;
    using Meta;
    using Ranges;

    public abstract class DatabaseConnection : Adapters.DatabaseConnection
    {
        private readonly Func<IWorkspaceServices> servicesBuilder;
        private readonly HashSet<Workspace> workspaces = new HashSet<Workspace>();
        private readonly HashSet<Session> sessions = new HashSet<Session>();
        private readonly List<IWorkspaceServices> workspaceServices = new List<IWorkspaceServices>();
        private readonly List<ISessionServices> sessionServices = new List<ISessionServices>();
        private readonly List<IDisposable> ownedServices = new List<IDisposable>();
        private Lifecycle lifecycle;
        private bool initializationStarted;
        private int constructions;
        private TaskCompletionSource<bool> constructionDrained;
        private TaskCompletionSource<bool> closed;

        protected DatabaseConnection(Adapters.Configuration configuration, IdGenerator idGenerator, Func<IWorkspaceServices> servicesBuilder, IRanges<long> ranges)
            : base(configuration ?? throw new ArgumentNullException(nameof(configuration)), idGenerator)
        {
            this.Ranges = ranges ?? throw new ArgumentNullException(nameof(ranges));
            this.servicesBuilder = servicesBuilder ?? throw new ArgumentNullException(nameof(servicesBuilder));
        }

        internal object State { get; } = new object();
        internal ScopedConnection ScopedOwner { get; private set; }
        internal bool IsClosing { get { lock (this.State) return this.lifecycle == Lifecycle.Closing || this.lifecycle == Lifecycle.Closed; } }
        internal void RequireOperational()
        {
            if (this.IsClosing) throw new InvalidOperationException("This connection is closing or closed. Create a replacement connection.");
        }
        internal IRanges<long> Ranges { get; }
        public abstract IUnitConvert UnitConvert { get; }

        // Retained only for the independent legacy wire record adapter. Normal Sessions never
        // accept identity, records or permission authority from a connection-wide cache.
        internal Dictionary<long, AccessControl> AccessControlById { get; } = new Dictionary<long, AccessControl>();
        internal Dictionary<long, Revocation> RevocationById { get; } = new Dictionary<long, Revocation>();
        public override Adapters.DatabaseRecord GetRecord(long id) => null;
        public override long GetPermission(IClass @class, IOperandType operandType, Operations operation) => 0;

        protected async Task InitializeAsync(IReadTransport readTransport, string revision, string context = null,
            IPushReadTransport pushReadTransport = null, IInvokeReadTransport invokeReadTransport = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                lock (this.State)
                {
                    if (this.initializationStarted || this.lifecycle != Lifecycle.Initializing)
                        throw new InvalidOperationException("Initialize a connection exactly once before publishing it.");
                    this.initializationStarted = true;
                    cancellationToken.ThrowIfCancellationRequested();
                    if (readTransport == null || pushReadTransport == null || invokeReadTransport == null)
                        throw new ArgumentException("Supply complete read, protected Push and protected Invoke transports before creating a connection.");
                    this.ScopedOwner = new ScopedConnection(this, readTransport, this.Configuration.Name, revision, context, pushReadTransport, invokeReadTransport);
                    this.ScopedOwner.ConfigurePushMetadata(this.Configuration.MetaPopulation);
                }
                await this.ScopedOwner.InitializeAsync(cancellationToken).ConfigureAwait(false);
                lock (this.State)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (this.lifecycle != Lifecycle.Initializing)
                        throw new InvalidOperationException("Connection creation ended during shutdown. Create a new connection.");
                    this.lifecycle = Lifecycle.Ready;
                }
            }
            catch
            {
                await this.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }

        internal void RequireReady()
        {
            lock (this.State)
            {
                if (this.lifecycle == Lifecycle.Closing || this.lifecycle == Lifecycle.Closed)
                    throw new ObjectDisposedException(nameof(DatabaseConnection), "This connection is closing or closed. Create a new connection.");
                if (this.lifecycle != Lifecycle.Ready)
                    throw new InvalidOperationException("This connection is not ready or is closing. Await connection creation before use, or create a new connection after shutdown.");
            }
        }

        private void ReserveConstruction()
        {
            lock (this.State)
            {
                this.RequireReady();
                if (this.constructions++ == 0)
                    this.constructionDrained = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }

        private void FinishConstruction()
        {
            lock (this.State)
            {
                if (--this.constructions == 0) this.constructionDrained.TrySetResult(true);
            }
        }

        public override IWorkspace CreateWorkspace()
        {
            this.ReserveConstruction();
            Workspace workspace = null;
            try
            {
                var services = this.servicesBuilder() ?? throw new InvalidOperationException("The workspace services factory must return a fresh service instance.");
                lock (this.State)
                {
                    if (this.ownedServices.Any(value => ReferenceEquals(value, services))) throw new InvalidOperationException("Return a fresh workspace service instance for each workspace.");
                    this.ownedServices.Add(services);
                    this.workspaceServices.Add(services);
                    this.RequireReady();
                    workspace = new Workspace(this, services, this.Ranges);
                    this.workspaces.Add(workspace);
                }
                services.OnInit(workspace);
                lock (this.State) { this.RequireReady(); workspace.RequireUsable(); }
                return workspace;
            }
            catch { workspace?.Close(); throw; }
            finally { this.FinishConstruction(); }
        }

        internal ISession CreateSession(Workspace workspace)
        {
            this.ReserveConstruction();
            Session session = null;
            try
            {
                lock (this.State)
                {
                    if (!this.workspaces.Contains(workspace)) throw new InvalidOperationException("Create a Session through its owning connection's workspace.");
                    workspace.RequireUsable();
                }
                var services = workspace.Services.CreateSessionServices() ?? throw new InvalidOperationException("The Session services factory must return a fresh service instance.");
                lock (this.State)
                {
                    if (this.ownedServices.Any(value => ReferenceEquals(value, services))) throw new InvalidOperationException("Return a fresh Session service instance for each Session.");
                    this.ownedServices.Add(services);
                    this.sessionServices.Add(services);
                    this.RequireReady();
                    workspace.RequireUsable();
                    session = new Session(workspace, services);
                    this.sessions.Add(session);
                }
                services.OnInit(session);
                lock (this.State) { this.RequireReady(); session.RequireUsable(); }
                return session;
            }
            catch { session?.Dispose(); throw; }
            finally { this.FinishConstruction(); }
        }

        public override Task DisposeAsync()
        {
            lock (this.State)
            {
                if (this.closed != null) return this.closed.Task;
                this.closed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                this.lifecycle = Lifecycle.Closing;
                _ = this.CloseAsync();
                return this.closed.Task;
            }
        }

        private async Task CloseAsync()
        {
            var errors = new List<Exception>();
            try
            {
                try { if (this.ScopedOwner != null) await this.ScopedOwner.DrainAsync().ConfigureAwait(false); }
                catch (Exception error) { errors.Add(error); }
                finally { this.ScopedOwner?.Clear(); }
                Task pending;
                lock (this.State) pending = this.constructions == 0 ? Task.CompletedTask : this.constructionDrained.Task;
                await pending.ConfigureAwait(false);
                lock (this.State)
                {
                    foreach (var session in this.sessions)
                        try { session.Dispose(); } catch (Exception error) { errors.Add(error); }
                    foreach (var workspace in this.workspaces) workspace.Close();
                }
                // Session resources may be used by already dispatched work, including an early
                // closed Session. Keep all ownership until after the shared operation drain.
                foreach (var services in this.sessionServices)
                    try { services.Dispose(); } catch (Exception error) { errors.Add(error); }
                foreach (var services in this.workspaceServices)
                    try { services.Dispose(); } catch (Exception error) { errors.Add(error); }
            }
            catch (Exception error) { errors.Add(error); }
            finally
            {
                lock (this.State)
                {
                    this.lifecycle = Lifecycle.Closed;
                    if (errors.Count == 0) this.closed.TrySetResult(true);
                    else this.closed.TrySetException(new AggregateException("Connection cleanup failed. Every registered resource was given a cleanup attempt.", errors));
                }
            }
        }

        private enum Lifecycle { Initializing, Ready, Closing, Closed }
    }
}
