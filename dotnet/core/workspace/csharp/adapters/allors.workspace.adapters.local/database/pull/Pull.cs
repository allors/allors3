// <copyright file="LocalPullResult.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Database.Data;
    using Database.Domain;
    using Database.Meta;
    using Database.Security;
    using Protocol.Direct;
    using IObject = Allors.Workspace.IObject;
    using User = Database.Domain.User;

    public class Pull : Result, IPullResult, IProcedureOutput
    {
        private IDictionary<string, IObject[]> collections;
        private IDictionary<string, IObject> objects;

        public Pull(Session session, Workspace workspace) : base(session)
        {
            this.Workspace = workspace;
            this.Transaction = this.Workspace.DatabaseConnection.Database.CreateTransaction();

            var databaseContext = this.Transaction.Database.Services();
            var metaCache = databaseContext.MetaCache;

            var user = (User)this.Transaction.Instantiate(this.Workspace.UserId);

            this.AccessControlLists = new WorkspaceAccessControlLists(this.Workspace.DatabaseConnection.Configuration.Name, user);
            this.AllowedClasses = metaCache.GetWorkspaceClasses(this.Workspace.DatabaseConnection.Configuration.Name);
            this.PreparedSelects = databaseContext.PreparedSelects;
            this.PreparedExtents = databaseContext.PreparedExtents;

            this.DatabaseObjects = new HashSet<Database.IObject>();
        }

        public IAccessControlLists AccessControlLists { get; }

        public HashSet<Database.IObject> DatabaseObjects { get; }

        private Dictionary<string, ISet<Database.IObject>> DatabaseCollectionsByName { get; } = new Dictionary<string, ISet<Database.IObject>>();

        private Dictionary<string, Database.IObject> DatabaseObjectByName { get; } = new Dictionary<string, Database.IObject>();

        private Dictionary<string, object> ValueByName { get; } = new Dictionary<string, object>();

        private Workspace Workspace { get; }

        private ITransaction Transaction { get; }

        private ISet<IClass> AllowedClasses { get; }

        private IPreparedSelects PreparedSelects { get; }

        private IPreparedExtents PreparedExtents { get; }

        public void AddCollection(string name, in IEnumerable<Database.IObject> collection, Node[] tree)
        {
            switch (collection)
            {
                case ICollection<Database.IObject> list:
                    this.AddCollectionInternal(name, list, tree);
                    break;
                default:
                {
                    this.AddCollectionInternal(name, collection.ToArray(), tree);
                    break;
                }
            }
        }

        public void AddObject(string name, Database.IObject @object, Node[] tree)
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

                    this.DatabaseObjects.Add(@object);
                    this.DatabaseObjectByName[name] = @object;
                    tree?.Resolve(@object, this.AccessControlLists, this.DatabaseObjects);
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

        public IDictionary<string, IObject[]> Collections =>
            this.collections ??= this.DatabaseCollectionsByName.ToDictionary(v => v.Key,
                v => v.Value.Select(w => this.Session.GetOne<IObject>(w.Id)).ToArray());

        public IDictionary<string, IObject> Objects =>
            this.objects ??= this.DatabaseObjectByName.ToDictionary(v => v.Key,
                v => this.Session.GetOne<IObject>(v.Value.Id));

        public IDictionary<string, object> Values => this.ValueByName;

        public T[] GetCollection<T>() where T : IObject
        {
            var objectType = this.Workspace.DatabaseConnection.Configuration.ObjectFactory.GetObjectType<T>();
            var key = objectType.PluralName;
            return this.GetCollection<T>(key);
        }

        public T[] GetCollection<T>(string key) where T : IObject =>
            this.Collections.TryGetValue(key, out var collection) ? collection?.Cast<T>().ToArray() : null;

        public T GetObject<T>()
            where T : class, IObject
        {
            var objectType = this.Workspace.DatabaseConnection.Configuration.ObjectFactory.GetObjectType<T>();
            var key = objectType.SingularName;
            return this.GetObject<T>(key);
        }

        public T GetObject<T>(string key)
            where T : class, IObject => this.Objects.TryGetValue(key, out var @object) ? (T)@object : null;

        public object GetValue(string key) => this.Values[key];

        public T GetValue<T>(string key) => (T)this.GetValue(key);

        public void Execute(Allors.Workspace.Data.Procedure workspaceProcedure)
        {
            var visitor = new ToDatabaseVisitor(this.Transaction);
            var procedure = visitor.Visit(workspaceProcedure);
            var localProcedure = new Procedure(this.Transaction, procedure, this.AccessControlLists);
            localProcedure.Execute(this);
        }

        public void Execute(IEnumerable<Allors.Workspace.Data.Pull> workspacePulls)
        {
            var visitor = new ToDatabaseVisitor(this.Transaction);
            var pulls = workspacePulls.Select(v => visitor.Visit(v));

            foreach (var pull in pulls)
            {
                if (pull.Object != null)
                {
                    var pullInstantiate = new PullInstantiate(this.Transaction, pull, this.AccessControlLists,
                        this.PreparedSelects);
                    pullInstantiate.Execute(this);
                }
                else
                {
                    var pullExtent = new PullExtent(this.Transaction, pull, this.AccessControlLists,
                        this.PreparedSelects, this.PreparedExtents);
                    pullExtent.Execute(this);
                }
            }
        }
        
        public void AddCollection(string name, in ICollection<Database.IObject> collection) =>
            this.AddCollectionInternal(name, collection, null);

        public void AddObject(string name, Database.IObject @object)
        {
            if (@object != null)
            {
                this.AddObject(name, @object, null);
            }
        }

        private void AddCollectionInternal(string name, in ICollection<Database.IObject> collection, Node[] tree)
        {
            if (collection?.Count > 0)
            {
                this.DatabaseCollectionsByName.TryGetValue(name, out var existingCollection);

                var filteredCollection = collection.Where(v =>
                    this.AllowedClasses != null && this.AllowedClasses.Contains(v.Strategy.Class));

                if (tree != null)
                {
                    var prefetchPolicy = tree.BuildPrefetchPolicy();

                    ICollection<Database.IObject> newCollection;

                    if (existingCollection != null)
                    {
                        newCollection = filteredCollection.ToArray();
                        this.Transaction.Prefetch(prefetchPolicy, newCollection);
                        existingCollection.UnionWith(newCollection);
                    }
                    else
                    {
                        var newSet = new HashSet<Database.IObject>(filteredCollection);
                        newCollection = newSet;
                        this.Transaction.Prefetch(prefetchPolicy, newCollection);
                        this.DatabaseCollectionsByName.Add(name, newSet);
                    }

                    this.DatabaseObjects.UnionWith(newCollection);

                    foreach (var newObject in newCollection)
                    {
                        tree.Resolve(newObject, this.AccessControlLists, this.DatabaseObjects);
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
                        var newWorkspaceCollection = new HashSet<Database.IObject>(filteredCollection);
                        this.DatabaseCollectionsByName.Add(name, newWorkspaceCollection);
                        this.DatabaseObjects.UnionWith(newWorkspaceCollection);
                    }
                }
            }
        }
    }
}
