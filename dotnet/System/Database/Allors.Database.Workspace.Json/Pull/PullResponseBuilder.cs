// <copyright file="PullResponseBuilder.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Pull;
    using Data;
    using Domain;
    using Meta;
    using Ranges;
    using Security;
    using Services;

    public class PullResponseBuilder
    {
        private readonly IUnitConvert unitConvert;
        private readonly IRanges<long> ranges;
        private readonly IDictionary<IClass, ISet<IPropertyType>> dependencies;

        private readonly Dictionary<string, ISet<IObject>> collectionsByName = new Dictionary<string, ISet<IObject>>();
        private readonly Dictionary<string, IObject> objectByName = new Dictionary<string, IObject>();

        private readonly HashSet<IObject> includedObjects;

        private readonly IPrefetchPolicyCache prefetchPolicyCache;

        public PullResponseBuilder(
            ITransaction transaction,
            IAccessControl accessControl,
            ISet<IClass> allowedClasses,
            IUnitConvert unitConvert,
            IRanges<long> ranges,
            IDictionary<IClass, ISet<IPropertyType>> dependencies,
            IPrefetchPolicyCache prefetchPolicyCache,
            CancellationToken cancellationToken)
        {
            if (transaction.Database is not Adapters.Sql.Database)
            {
                throw new NotSupportedException("Workspace operations require PostgreSQL or SQL Server. Memory remains available for database and domain tests.");
            }

            this.unitConvert = unitConvert;
            this.ranges = ranges;
            this.dependencies = dependencies;
            this.Transaction = transaction;

            this.AccessControl = accessControl;
            this.AllowedClasses = allowedClasses;
            this.CancellationToken = cancellationToken;

            this.prefetchPolicyCache = prefetchPolicyCache;

            this.includedObjects = new HashSet<IObject>();
        }

        public ITransaction Transaction { get; }

        public IAccessControl AccessControl { get; }

        public ISet<IClass> AllowedClasses { get; }

        public CancellationToken CancellationToken { get; }

        public void AddCollection(string name, IComposite objectType, in IEnumerable<IObject> collection) => this.AddCollectionInternal(name, objectType, collection, null);

        public void AddCollection(string name, IComposite objectType, in IEnumerable<IObject> collection, Node[] tree)
        {
            switch (collection)
            {
                case ICollection<IObject> list:
                    this.AddCollectionInternal(name, objectType, list, tree);
                    break;
                default:
                {
                    this.AddCollectionInternal(name, objectType, collection.ToArray(), tree);
                    break;
                }
            }
        }

        public void AddObject(string name, IObject @object) => this.AddObjectInternal(name, @object);

        public void AddObject(string name, IObject @object, Node[] tree) => this.AddObjectInternal(name, @object, tree);

        private void AddObjectInternal(string name, IObject @object, Node[] tree = null)
        {
            if (@object == null)
            {
                return;
            }

            this.Transaction.Prefetch(this.prefetchPolicyCache.Security, @object);

            if (!this.Include(@object))
            {
                return;
            }

            this.includedObjects.Add(@object);
            this.objectByName[name] = @object;
            tree?.Resolve(@object, this.AccessControl, this.Add, this.prefetchPolicyCache, this.Transaction);
        }

        private void AddCollectionInternal(string name, IComposite objectType, in IEnumerable<IObject> enumerable, Node[] tree)
        {
            var collection = enumerable as ICollection<IObject> ?? enumerable.ToArray();

            if (collection.Count == 0)
            {
                return;
            }

            this.Transaction.Prefetch(this.prefetchPolicyCache.Security, collection);
            var includedCollection = collection.Where(this.Include);

            this.collectionsByName.TryGetValue(name, out var existingCollection);

            if (tree != null)
            {
                ICollection<IObject> newCollection;

                if (existingCollection != null)
                {
                    newCollection = includedCollection.ToArray();
                    existingCollection.UnionWith(newCollection);
                }
                else
                {
                    var newSet = new HashSet<IObject>(includedCollection);
                    newCollection = newSet;
                    this.collectionsByName.Add(name, newSet);
                }

                this.includedObjects.UnionWith(newCollection);

                tree.Resolve(newCollection, this.AccessControl, this.Add, this.prefetchPolicyCache, this.Transaction);
            }
            else
            {
                if (existingCollection != null)
                {
                    existingCollection.UnionWith(includedCollection);
                }
                else
                {
                    var newWorkspaceCollection = new HashSet<IObject>(includedCollection);
                    this.collectionsByName.Add(name, newWorkspaceCollection);
                    this.includedObjects.UnionWith(newWorkspaceCollection);
                }
            }
        }

        public PullResponse Build(PullRequest pullRequest = null)
        {
            if (pullRequest?.l != null)
            {
                var pulls = pullRequest.l.FromJson(this.Transaction, this.unitConvert);

                foreach (var pull in pulls)
                {
                    this.ThrowIfCancellationRequested();

                    if (pull.Object != null)
                    {
                        var pullInstantiate = new PullInstantiate(this.Transaction, pull, this.AccessControl);
                        pullInstantiate.Execute(this);
                    }
                    else
                    {
                        var pullExtent = new PullExtent(this.Transaction, pull, this.AccessControl, this.prefetchPolicyCache);
                        pullExtent.Execute(this);
                    }
                }
            }

            // Add dependencies
            this.AddDependencies();

            // Serialize
            var versionByGrant = new Dictionary<long, long>();
            var versionByRevocation = new Dictionary<long, long>();

            this.ThrowIfCancellationRequested();

            var pullResponse = new PullResponse
            {
                p = this.includedObjects.Select(v =>
                {
                    var accessControlList = this.AccessControl[v];
                    var grants = accessControlList.Grants;
                    var revocations = accessControlList.Revocations;

                    foreach (var grant in grants)
                    {
                        versionByGrant[grant.Id] = grant.Version;
                    }

                    foreach (var revocation in revocations)
                    {
                        versionByRevocation[revocation.Id] = revocation.Version;
                    }

                    return new PullResponseObject
                    {
                        i = v.Strategy.ObjectId,
                        v = v.Strategy.ObjectVersion,
                        g = this.ranges.Import(grants.Select(w => w.Id)).Save(),
                        r = this.ranges.Import(revocations.Select(w => w.Id))
                            .Save(),
                    };
                }).ToArray(),
                o = this.objectByName.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Id),
                c = this.collectionsByName.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(obj => obj.Id).ToArray()),
            };

            pullResponse.g = versionByGrant.Count > 0 ? versionByGrant.Select(v => new[] { v.Key, v.Value }).ToArray() : null;
            pullResponse.r = versionByRevocation.Count > 0 ? versionByRevocation.Select(v => new[] { v.Key, v.Value }).ToArray() : null;

            return pullResponse;
        }

        public bool Include(IObject @object) => @object != null && this.AllowedClasses?.Contains(@object.Strategy.Class) == true;

        private void AddDependencies()
        {
            if (this.dependencies == null)
            {
                return;
            }

            this.ThrowIfCancellationRequested();

            var current = this.includedObjects.ToArray();

            while (current.Length > 0)
            {
                this.ThrowIfCancellationRequested();

                var newObjects = new HashSet<IObject>();

                foreach (var grouping in current.GroupBy(v => v.Strategy.Class, v => v))
                {
                    this.ThrowIfCancellationRequested();

                    var @class = grouping.Key;
                    var grouped = grouping.ToArray();

                    if (this.dependencies.TryGetValue(@class, out var propertyTypes))
                    {
                        var prefetchPolicy = this.prefetchPolicyCache.ForDependency(@class, propertyTypes);
                        this.Transaction.Prefetch(prefetchPolicy, grouped);

                        foreach (var @object in grouped)
                        {
                            foreach (var propertyType in propertyTypes)
                            {
                                if (propertyType is IRoleType roleType)
                                {
                                    if (roleType.IsOne)
                                    {
                                        var role = @object.Strategy.GetCompositeRole(roleType);
                                        if (this.Include(role))
                                        {
                                            newObjects.Add(role);
                                        }
                                    }
                                    else
                                    {
                                        var role = @object.Strategy.GetCompositesRole<IObject>(roleType).Where(this.Include);
                                        newObjects.UnionWith(role);
                                    }
                                }
                                else
                                {
                                    var associationType = (IAssociationType)propertyType;
                                    if (associationType.IsOne)
                                    {
                                        var association = @object.Strategy.GetCompositeAssociation(associationType);
                                        if (this.Include(association))
                                        {
                                            newObjects.Add(association);
                                        }
                                    }
                                    else
                                    {
                                        var association = @object.Strategy.GetCompositesAssociation<IObject>(associationType).Where(this.Include);
                                        newObjects.UnionWith(association);
                                    }
                                }
                            }
                        }
                    }
                }

                current = newObjects.Except(this.includedObjects).ToArray();
                this.includedObjects.UnionWith(newObjects);
            }
        }

        private void Add(IObject @object)
        {
            if (this.Include(@object))
            {
                this.includedObjects.Add(@object);
            }
        }

        private void ThrowIfCancellationRequested()
        {
            //if (this.CancellationToken.IsCancellationRequested)
            //{
            //    Debugger.Break();
            //}

            this.CancellationToken.ThrowIfCancellationRequested();
        }
    }
}
