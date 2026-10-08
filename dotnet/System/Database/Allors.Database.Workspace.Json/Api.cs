// <copyright file="Api.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Allors.Protocol.Json.Api;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Protocol.Json.SystemTextJson;
    using Data;
    using Derivations;
    using Domain;
    using Meta;
    using Ranges;
    using Security;
    using Services;
    using Tracing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// The entry point of the JSON protocol: the six calls, each on the transaction, the
    /// workspace name and the user the Api was created with. Every response says which database
    /// and user it is from and which workspace name and meta fingerprint the server used; a
    /// request that names another workspace or another fingerprint is refused with the reason in
    /// its error message, a request that names neither is served.
    /// </summary>
    public partial class Api
    {
        private readonly ILogger logger;

        public Api(ITransaction transaction, string workspaceName, CancellationToken cancellationToken, ILogger logger = null)
        {
            this.logger = logger ?? NullLogger.Instance;
            this.Transaction = transaction;
            this.WorkspaceName = workspaceName;
            this.CancellationToken = cancellationToken;
            this.Sink = transaction.Database.Sink;

            var transactionServices = transaction.Services;
            var databaseServices = transaction.Database.Services;
            var metaCache = databaseServices.Get<IMetaCache>();

            this.PrefetchPolicyCache = databaseServices.Get<IPrefetchPolicyCache>();
            this.Ranges = databaseServices.Get<IRanges<long>>();
            this.User = transactionServices.Get<IUserService>().User;
            this.AccessControl = transactionServices.Get<IWorkspaceAclsService>().Create(this.WorkspaceName);
            this.AllowedClasses = metaCache.GetWorkspaceClasses(this.WorkspaceName);
            this.RoleTypesByClass = metaCache.GetWorkspaceRoleTypesByClass(this.WorkspaceName);
            this.MetaFingerprint = metaCache.GetWorkspaceFingerprint(this.WorkspaceName);
            this.M = transaction.Database.MetaPopulation;
            this.MetaPopulation = this.M;
            this.PreparedSelects = databaseServices.Get<IPreparedSelects>();
            this.PreparedExtents = databaseServices.Get<IPreparedExtents>();
            this.Build = @class => transactionServices.Get<IObjectBuilderService>().Build(@class);
            this.Derive = () => databaseServices.Get<IDerivationService>().CreateDerivation(transaction).Derive();
            this.Security = databaseServices.Get<ISecurity>();

            this.UnitConvert = new UnitConvert();
        }

        public ITransaction Transaction { get; }

        public ISecurity Security { get; }

        public string WorkspaceName { get; set; }

        /// <summary>
        /// The fingerprint of the server's meta for <see cref="WorkspaceName"/>.
        /// </summary>
        public string MetaFingerprint { get; }

        public CancellationToken CancellationToken { get; }

        public ISink Sink { get; }

        public IPrefetchPolicyCache PrefetchPolicyCache { get; set; }

        public IRanges<long> Ranges { get; }

        public IUser User { get; }

        public IAccessControl AccessControl { get; }

        public ISet<IClass> AllowedClasses { get; }

        public IDictionary<IClass, ISet<IRoleType>> RoleTypesByClass { get; }

        public IMetaPopulation M { get; }

        public IMetaPopulation MetaPopulation { get; }

        public IPreparedSelects PreparedSelects { get; }

        public IPreparedExtents PreparedExtents { get; }

        public Func<IClass, IObject> Build { get; }

        public Func<IValidation> Derive { get; }

        public UnitConvert UnitConvert { get; }

        public InvokeResponse Invoke(InvokeRequest invokeRequest)
        {
            if (this.Refuses(invokeRequest, out var refusal))
            {
                return this.Envelope(new InvokeResponse { _e = refusal });
            }

            var @event = this.Sink?.OnInvoke(this.Transaction, invokeRequest);
            this.Sink?.OnBefore(@event);

            var invokeResponseBuilder = new InvokeResponseBuilder(this.Transaction, this.Derive, this.AccessControl, this.AllowedClasses);
            var invokeResponse = invokeResponseBuilder.Build(invokeRequest);

            if (@event != null)
            {
                @event.InvokeResponse = invokeResponse;
                this.Sink?.OnAfter(@event);
            }

            return this.Envelope(invokeResponse);
        }

        public PullResponse Pull(PullRequest pullRequest)
        {
            if (this.Refuses(pullRequest, out var refusal))
            {
                return this.Envelope(new PullResponse { _e = refusal });
            }

            var @event = this.Sink?.OnPull(this.Transaction, pullRequest);
            this.Sink?.OnBefore(@event);

            var dependencies = this.ToDependencies(pullRequest.d);
            var pullResponseBuilder = new PullResponseBuilder(this.Transaction, this.AccessControl, this.AllowedClasses, this.PreparedSelects, this.PreparedExtents, this.UnitConvert, this.Ranges, dependencies, this.PrefetchPolicyCache, this.CancellationToken);
            var pullResponse = pullResponseBuilder.Build(pullRequest);

            if (@event != null)
            {
                @event.PullResponse = pullResponse;
                this.Sink?.OnAfter(@event);
            }

            return this.Envelope(pullResponse);
        }

        public PushResponse Push(PushRequest pushRequest)
        {
            if (this.Refuses(pushRequest, out var refusal))
            {
                return this.Envelope(new PushResponse { _e = refusal });
            }

            var @event = this.Sink?.OnPush(this.Transaction, pushRequest);
            this.Sink?.OnBefore(@event);

            var pushResponseBuilder = new PushResponseBuilder(this.Transaction, this.Derive, this.MetaPopulation, this.AccessControl, this.AllowedClasses, this.Build, this.UnitConvert);
            var pushResponse = pushResponseBuilder.Build(pushRequest);

            if (@event != null)
            {
                @event.PushResponse = pushResponse;
                this.Sink?.OnAfter(@event);
            }

            return this.Envelope(pushResponse);
        }

        public SyncResponse Sync(SyncRequest syncRequest)
        {
            if (this.Refuses(syncRequest, out var refusal))
            {
                return this.Envelope(new SyncResponse { _e = refusal });
            }

            var @event = this.Sink?.OnSync(this.Transaction, syncRequest);
            this.Sink?.OnBefore(@event);

            var prefetchPolicyByClass = this.PrefetchPolicyCache.WorkspacePrefetchPolicyByClass(this.WorkspaceName);
            var syncResponseBuilder = new SyncResponseBuilder(this.Transaction, this.AccessControl, this.AllowedClasses, this.RoleTypesByClass, prefetchPolicyByClass, this.UnitConvert, this.Ranges);
            var syncResponse = syncResponseBuilder.Build(syncRequest);

            if (@event != null)
            {
                @event.SyncResponse = syncResponse;
                this.Sink?.OnAfter(@event);
            }

            return this.Envelope(syncResponse);
        }

        public AccessResponse Access(AccessRequest accessRequest)
        {
            if (this.Refuses(accessRequest, out var refusal))
            {
                return this.Envelope(new AccessResponse { _e = refusal });
            }

            var responseBuilder = new AccessResponseBuilder(this.Transaction, this.Security, this.User, this.WorkspaceName);
            return this.Envelope(responseBuilder.Build(accessRequest));
        }

        public PermissionResponse Permission(PermissionRequest permissionRequest)
        {
            if (this.Refuses(permissionRequest, out var refusal))
            {
                return this.Envelope(new PermissionResponse { _e = refusal });
            }

            var responseBuilder = new PermissionResponseBuilder(this.Transaction, this.AllowedClasses);
            return this.Envelope(responseBuilder.Build(permissionRequest));
        }

        /// <summary>
        /// A pull response builder for a named pull: a route of the server that fills the
        /// response itself. The response it builds carries the envelope.
        /// </summary>
        public PullResponseBuilder CreatePullResponseBuilder(string dependencyId = null)
        {
            // TODO: Dependencies
            return new PullResponseBuilder(this.Transaction, this.AccessControl, this.AllowedClasses, this.PreparedSelects, this.PreparedExtents, this.UnitConvert, this.Ranges, null, this.PrefetchPolicyCache, this.CancellationToken, response => this.Envelope(response));
        }

        /// <summary>
        /// Says which database and user the response is from and which workspace name and meta
        /// fingerprint the server used.
        /// </summary>
        public T Envelope<T>(T response)
            where T : Response
        {
            response._db = this.Transaction.Database.Id;
            response._u = this.User?.Id;
            response._w = this.WorkspaceName;
            response._f = this.MetaFingerprint;
            return response;
        }

        private bool Refuses(Request request, out string reason)
        {
            if (request._w != null && !string.Equals(request._w, this.WorkspaceName, StringComparison.Ordinal))
            {
                reason = $"The request is for workspace '{request._w}' but this server serves workspace '{this.WorkspaceName}' for this host. Send the request to the host that serves '{request._w}', or build the client for '{this.WorkspaceName}'.";
                return true;
            }

            if (request._f != null && !string.Equals(request._f, this.MetaFingerprint, StringComparison.Ordinal))
            {
                reason = $"The request's workspace meta has fingerprint {request._f} but this server's workspace '{this.WorkspaceName}' has fingerprint {this.MetaFingerprint}: the client's workspace meta was generated from another version of the domain. Regenerate the client's workspace meta from this server's repository.";
                return true;
            }

            reason = null;
            return false;
        }

        private IDictionary<IClass, ISet<IPropertyType>> ToDependencies(PullDependency[] pullDependencies)
        {
            if (pullDependencies == null)
            {
                return null;
            }

            var classDependencies = new Dictionary<IClass, ISet<IPropertyType>>();

            foreach (var pullDependency in pullDependencies)
            {
                if (this.M.FindByTag(pullDependency.o) is not IComposite objectType)
                {
                    this.LogIgnoredObjectTypeTag(pullDependency.o);
                    continue;
                }

                IPropertyType propertyType;
                if (pullDependency.a != null)
                {
                    if (this.M.FindByTag(pullDependency.a) is not IRelationType associationRelationType)
                    {
                        this.LogIgnoredAssociationTag(pullDependency.a);
                        continue;
                    }

                    propertyType = associationRelationType.AssociationType;
                }
                else
                {
                    if (this.M.FindByTag(pullDependency.r) is not IRelationType roleRelationType)
                    {
                        this.LogIgnoredRoleTag(pullDependency.r);
                        continue;
                    }

                    propertyType = roleRelationType.RoleType;
                }

                foreach (var @class in objectType.Classes)
                {
                    if (!classDependencies.TryGetValue(@class, out var classDependency))
                    {
                        classDependency = new HashSet<IPropertyType>();
                        classDependencies.Add(@class, classDependency);
                    }

                    classDependency.Add(propertyType);
                }
            }

            return classDependencies;
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Ignoring pull dependency: unknown or non-composite object type tag {ObjectTypeTag}")]
        private partial void LogIgnoredObjectTypeTag(string objectTypeTag);

        [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Ignoring pull dependency: unknown or non-relation association tag {AssociationTag}")]
        private partial void LogIgnoredAssociationTag(string associationTag);

        [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Ignoring pull dependency: unknown or non-relation role tag {RoleTag}")]
        private partial void LogIgnoredRoleTag(string roleTag);
    }
}
