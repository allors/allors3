// <copyright file="LocalPullResult.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Direct
{
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Database.Data;
    using Database.Domain;
    using Database.Security;
    using Protocol.Direct;
    using IClass = Database.Meta.IClass;
    using Node = Database.Data.Node;
    using Pull = Database.Data.Pull;

    public class LocalPullResult
    {
        public LocalPullResult(LocalWorkspace workspace)
        {
            this.Workspace = workspace;
            this.Transaction = this.Workspace.Database.CreateTransaction();

            var sessionContext = this.Transaction.Context();
            var databaseContext = this.Transaction.Database.Context();
            var metaCache = databaseContext.MetaCache;
            var user = sessionContext.User;

            this.AccessControlLists = new WorkspaceAccessControlLists(this.Workspace.Name, user);
            this.AllowedClasses = metaCache.GetWorkspaceClasses(this.Workspace.Name);
            this.PreparedFetches = databaseContext.PreparedFetches;
            this.PreparedExtents = databaseContext.PreparedExtents;

            this.Objects = new HashSet<IObject>();
        }

        public Dictionary<string, ISet<IObject>> CollectionsByName { get; } = new Dictionary<string, ISet<IObject>>();

        public Dictionary<string, IObject> ObjectByName { get; } = new Dictionary<string, IObject>();

        public Dictionary<string, object> ValueByName { get; } = new Dictionary<string, object>();

        public HashSet<IObject> Objects { get; }

        public LocalWorkspace Workspace { get; }

        public ITransaction Transaction { get; }

        public IAccessControlLists AccessControlLists { get; }

        public ISet<IClass> AllowedClasses { get; }

        public IPreparedFetches PreparedFetches { get; }

        public IPreparedExtents PreparedExtents { get; }

        public void AddCollection(string name, in IEnumerable<IObject> collection)
        {
            switch (collection)
            {
                case ICollection<IObject> asCollection:
                    this.AddCollectionInternal(name, asCollection, null);
                    break;
                default:
                    this.AddCollectionInternal(name, collection.ToArray(), null);
                    break;
            }
        }

        public void AddCollection(string name, in ICollection<IObject> collection) => this.AddCollectionInternal(name, collection, null);

        public void AddCollection(string name, IEnumerable<IObject> collection, Node[] tree)
        {
            switch (collection)
            {
                case ICollection<IObject> list:
                    this.AddCollectionInternal(name, list, tree);
                    break;
                default:
                {
                    this.AddCollectionInternal(name, collection.ToArray(), tree);
                    break;
                }
            }
        }

        public void AddObject(string name, IObject @object)
        {
            if (@object != null)
            {
                this.AddObject(name, @object, null);
            }
        }

        public void AddObject(string name, IObject @object, Node[] tree)
        {
            if (@object != null)
            {
                if (this.AllowedClasses?.Contains(@object.Strategy.Class) == true)
                {
                    if (tree != null)
                    {
                        // Prefetch
                        var session = @object.Strategy.Transaction;
                        var prefetcher = tree.BuildPrefetchPolicy();
                        session.Prefetch(prefetcher, @object);
                    }

                    this.Objects.Add(@object);
                    this.ObjectByName[name] = @object;
                    tree?.Resolve(@object, this.AccessControlLists, this.Objects);
                }
            }
        }

        public void AddValue(string name, object value)
        {
            if (value != null)
            {
                this.ValueByName.Add(name, value);
            }
        }

        private void AddCollectionInternal(string name, in ICollection<IObject> collection, Node[] tree)
        {
            if (collection?.Count > 0)
            {
                this.CollectionsByName.TryGetValue(name, out var existingCollection);

                var filteredCollection = collection.Where(v => this.AllowedClasses != null && this.AllowedClasses.Contains(v.Strategy.Class));

                if (tree != null)
                {
                    var prefetchPolicy = tree.BuildPrefetchPolicy();

                    ICollection<IObject> newCollection;

                    if (existingCollection != null)
                    {
                        newCollection = filteredCollection.ToArray();
                        this.Transaction.Prefetch(prefetchPolicy, newCollection);
                        existingCollection.UnionWith(newCollection);
                    }
                    else
                    {
                        var newSet = new HashSet<IObject>(filteredCollection);
                        newCollection = newSet;
                        this.Transaction.Prefetch(prefetchPolicy, newCollection);
                        this.CollectionsByName.Add(name, newSet);
                    }

                    this.Objects.UnionWith(newCollection);

                    foreach (var newObject in newCollection)
                    {
                        tree.Resolve(newObject, this.AccessControlLists, this.Objects);
                    }
                }
                else
                {
                    if (existingCollection != null)
                    {
                        existingCollection.UnionWith(filteredCollection);
                    }
                    else
                    {
                        var newWorkspaceCollection = new HashSet<IObject>(filteredCollection);
                        this.CollectionsByName.Add(name, newWorkspaceCollection);
                        this.Objects.UnionWith(newWorkspaceCollection);
                    }
                }
            }
        }

        public void Execute(IEnumerable<Allors.Workspace.Data.Pull> workspacePulls)
        {
            var visitor = new ToDatabaseVisitor(this.Transaction);
            var pulls = workspacePulls.Select(v => visitor.Visit(v));

            foreach (var pull in pulls)
            {
                if (pull.Object != null)
                {
                    var pullInstantiate = new LocalPullInstantiate(this.Transaction, pull, this.AccessControlLists, this.PreparedFetches);
                    pullInstantiate.Execute(this);
                }
                else
                {
                    var pullExtent = new LocalPullExtent(this.Transaction, pull, this.AccessControlLists, this.PreparedFetches, this.PreparedExtents);
                    pullExtent.Execute(this);
                }
            }
        }
    }
}
