// <copyright file="LocalPullResult.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Database.Data;
    using Database.Derivations;
    using Database.Domain;
    using Database.Meta;
    using Database.Security;
    using Protocol.Direct;
    using IClass = Database.Meta.IClass;
    using Node = Database.Data.Node;
    using Pull = Database.Data.Pull;

    public class LocalPushResult
    {
        internal LocalPushResult(LocalWorkspace workspace)
        {
            this.Workspace = workspace;
            this.Transaction = this.Workspace.Database.CreateTransaction();

            var sessionContext = this.Transaction.Context();
            var databaseContext = this.Transaction.Database.Context();
            var metaCache = databaseContext.MetaCache;
            var user = sessionContext.User;

            this.AccessControlLists = new WorkspaceAccessControlLists(this.Workspace.Name, user);
            this.AllowedClasses = metaCache.GetWorkspaceClasses(this.Workspace.Name);
            this.PreparedSelects = databaseContext.PreparedSelects;
            this.PreparedExtents = databaseContext.PreparedExtents;
            this.M = databaseContext.M;
            this.MetaPopulation = databaseContext.MetaPopulation;
            this.Build = @class => (IObject)DefaultObjectBuilder.Build(this.Transaction, @class);
            this.Derive = () => this.Transaction.Derive(false);

            // TODO: move to separate class
            this.AccessErrorStrategies = new List<LocalStrategy>();
            this.MissingIds = new List<long>();
            this.VersionErrors = new Dictionary<LocalStrategy, IObject>();

            this.Objects = new HashSet<IObject>();
        }

        public Dictionary<string, ISet<IObject>> CollectionsByName { get; } = new Dictionary<string, ISet<IObject>>();

        public Dictionary<string, IObject> ObjectByName { get; } = new Dictionary<string, IObject>();

        public Dictionary<string, object> ValueByName { get; } = new Dictionary<string, object>();

        public HashSet<IObject> Objects { get; }

        public LocalWorkspace Workspace { get; }

        public ITransaction Transaction { get; }

        public ISet<IClass> AllowedClasses { get; }

        public IPreparedSelects PreparedSelects { get; }

        public IPreparedExtents PreparedExtents { get; }

        internal IAccessControlLists AccessControlLists { get; }

        public M M { get; set; }

        public MetaPopulation MetaPopulation { get; set; }

        public Func<IClass, IObject> Build { get; }

        public Func<IDerivationResult> Derive { get; }

        public List<LocalStrategy> AccessErrorStrategies { get; }

        public List<long> MissingIds { get; }

        public Dictionary<LocalStrategy, IObject> VersionErrors { get; }

        public IDerivationResult Validation { get; set; }

        public bool HasErrors => this.AccessErrorStrategies?.Count > 0 || this.MissingIds?.Count > 0 || this.VersionErrors?.Count > 0 || this.Validation?.HasErrors == true;

        public Dictionary<long, IObject> ObjectByNewId { get; set; }

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

        internal void Execute(LocalStrategy[] newStrategies, LocalStrategy[] changedStrategies)
        {
            var metaPopulation = this.Workspace.Database.MetaPopulation;

            if (newStrategies?.Length > 0)
            {
                this.ObjectByNewId = newStrategies.ToDictionary(
                    x => x.Identity,
                    x =>
                    {
                        var cls = (IClass)metaPopulation.Find(x.Class.Id);
                        if (this.AllowedClasses?.Contains(cls) == true)
                        {
                            return (IObject)this.Build(cls);
                        }

                        this.AccessErrorStrategies.Add(x);

                        return null;
                    });
            }

            if (changedStrategies?.Length > 0)
            {
                // bulk load all objects
                var objectIds = changedStrategies.Select(v => v.Identity).ToArray();
                var objects = this.Transaction.Instantiate(objectIds);

                if (objectIds.Length != objects.Length)
                {
                    var existingIds = objects.Select(v => v.Id);
                    var missingIds = objectIds.Where(v => !existingIds.Contains(v));
                    foreach (var missingId in missingIds)
                    {
                        this.MissingIds.Add(missingId);
                    }
                }

                if (!this.HasErrors)
                {
                    foreach (var pushRequestObject in changedStrategies)
                    {
                        var obj = this.Transaction.Instantiate(pushRequestObject.Identity);

                        if (!pushRequestObject.DatabaseVersion.Equals(obj.Strategy.ObjectVersion))
                        {
                            this.VersionErrors[pushRequestObject] = obj;
                        }
                        else if (this.AllowedClasses?.Contains(obj.Strategy.Class) == true)
                        {
                            this.PushRequestRoles(pushRequestObject, obj);
                        }
                        else
                        {
                            this.AccessErrorStrategies.Add(pushRequestObject);
                        }
                    }
                }
            }

            this.Validation = this.Derive();

            if (!this.HasErrors)
            {

                this.Transaction.Commit();
            }
        }

        private void PushRequestRoles(LocalStrategy local, IObject obj)
        {
            // TODO: Cache and filter for workspace
            var composite = (IComposite)obj.Strategy.Class;
            var roleTypes = composite.DatabaseRoleTypes.Where(v => v.RelationType.WorkspaceNames.Length > 0);
            var acl = this.AccessControlLists[obj];

            foreach (var keyValuePair in local.DatabaseState.ChangedRoleByRelationType)
            {
                var relationType = keyValuePair.Key;
                var roleType = ((IRelationType)this.MetaPopulation.Find(keyValuePair.Key.Id)).RoleType;

                if (acl.CanWrite(roleType))
                {
                    var roleValue = keyValuePair.Value;

                    if (roleValue == null)
                    {
                        obj.Strategy.RemoveRole(roleType);
                    }
                    else
                    {
                        if (roleType.ObjectType.IsUnit)
                        {
                            obj.Strategy.SetUnitRole(roleType, roleValue);
                        }
                        else
                        {
                            if (relationType.RoleType.IsOne)
                            {
                                var role = this.GetRole((LocalStrategy)roleValue);
                                obj.Strategy.SetCompositeRole(roleType, role);
                            }
                            else
                            {
                                var role = this.GetRoles((LocalStrategy[])roleValue);
                                obj.Strategy.SetCompositeRoles(roleType, role);
                            }
                        }
                    }
                }
                else
                {
                    this.AccessErrorStrategies.Add(local);
                }
            }
        }

        private IObject GetRole(LocalStrategy localStrategy)
        {
            if (this.ObjectByNewId == null || !this.ObjectByNewId.TryGetValue(localStrategy.Identity, out var role))
            {
                role = this.Transaction.Instantiate(localStrategy.Identity);
            }

            return role;
        }

        private IObject[] GetRoles(LocalStrategy[] localStrategies)
        {
            if (this.ObjectByNewId == null)
            {
                return this.Transaction.Instantiate(localStrategies.Select(v => v.Identity));
            }

            var roles = new List<IObject>();
            List<long> existingRoleIds = null;
            foreach (var localStrategy in localStrategies)
            {
                if (this.ObjectByNewId.TryGetValue(localStrategy.Identity, out var role))
                {
                    roles.Add(role);
                }
                else
                {
                    existingRoleIds ??= new List<long>();
                    existingRoleIds.Add(localStrategy.Identity);
                }
            }

            if (existingRoleIds != null)
            {
                var existingRoles = this.Transaction.Instantiate(existingRoleIds);
                roles.AddRange(existingRoles);
            }

            return roles.ToArray();
        }
    }
}
