// <copyright file="LocalPullResult.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Data;
    using Database.Derivations;
    using Database.Domain;
    using Database.Meta;
    using Database.Security;
    using IClass = Database.Meta.IClass;
    using Node = Database.Data.Node;

    public class LocalPushResult : LocalResult, IPushResult
    {
        internal LocalPushResult(LocalSession session, LocalWorkspace workspace) : base(session)
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
            this.M = databaseContext.M;
            this.Build = @class => (Database.IObject)DefaultObjectBuilder.Build(this.Transaction, @class);
            this.Derive = () => this.Transaction.Derive(false);

            this.Objects = new HashSet<Database.IObject>();
        }

        public Dictionary<string, ISet<Database.IObject>> DatabaseCollectionsByName { get; } = new Dictionary<string, ISet<Database.IObject>>();

        public Dictionary<string, Database.IObject> DatabaseObjectByName { get; } = new Dictionary<string, Database.IObject>();

        public Dictionary<string, object> DatabaseValueByName { get; } = new Dictionary<string, object>();

        public HashSet<Database.IObject> Objects { get; }

        public LocalWorkspace Workspace { get; }

        public Database.ITransaction Transaction { get; }

        public ISet<IClass> AllowedClasses { get; }

        public IPreparedSelects PreparedSelects { get; }

        public IPreparedExtents PreparedExtents { get; }

        internal IAccessControlLists AccessControlLists { get; }

        public MetaPopulation M { get; set; }

        public Func<IClass, Database.IObject> Build { get; }

        public Func<IDerivationResult> Derive { get; }

        public Dictionary<long, Database.IObject> ObjectByNewId { get; set; }

        public void AddCollection(string name, in IEnumerable<Database.IObject> collection)
        {
            switch (collection)
            {
                case ICollection<Database.IObject> asCollection:
                    this.AddCollectionInternal(name, asCollection, null);
                    break;
                default:
                    this.AddCollectionInternal(name, collection.ToArray(), null);
                    break;
            }
        }

        public void AddCollection(string name, in ICollection<Database.IObject> collection) => this.AddCollectionInternal(name, collection, null);

        public void AddCollection(string name, IEnumerable<Database.IObject> collection, Node[] tree)
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

        public void AddObject(string name, Database.IObject @object)
        {
            if (@object != null)
            {
                this.AddObject(name, @object, null);
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

                    this.Objects.Add(@object);
                    this.DatabaseObjectByName[name] = @object;
                    tree?.Resolve(@object, this.AccessControlLists, this.Objects);
                }
            }
        }

        public void AddValue(string name, object value)
        {
            if (value != null)
            {
                this.DatabaseValueByName.Add(name, value);
            }
        }

        private void AddCollectionInternal(string name, in ICollection<Database.IObject> collection, Node[] tree)
        {
            if (collection?.Count > 0)
            {
                this.DatabaseCollectionsByName.TryGetValue(name, out var existingCollection);

                var filteredCollection = collection.Where(v => this.AllowedClasses != null && this.AllowedClasses.Contains(v.Strategy.Class));

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
                        var newWorkspaceCollection = new HashSet<Database.IObject>(filteredCollection);
                        this.DatabaseCollectionsByName.Add(name, newWorkspaceCollection);
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
                    x => x.Id,
                    x =>
                    {
                        var cls = (IClass)metaPopulation.FindByTag(x.Class.Tag);
                        if (this.AllowedClasses?.Contains(cls) == true)
                        {
                            return (Database.IObject)this.Build(cls);
                        }

                        this.AddAccessError(x);

                        return null;
                    });
            }

            if (changedStrategies?.Length > 0)
            {
                // bulk load all objects
                var objectIds = changedStrategies.Select(v => v.Id).ToArray();
                var objects = this.Transaction.Instantiate(objectIds);

                if (objectIds.Length != objects.Length)
                {
                    var existingIds = objects.Select(v => v.Id);
                    var missingIds = objectIds.Where(v => !existingIds.Contains(v));
                    foreach (var missingId in missingIds)
                    {
                        this.AddMissingId(missingId);
                    }
                }

                if (!this.HasErrors)
                {
                    foreach (var pushRequestObject in changedStrategies)
                    {
                        var obj = this.Transaction.Instantiate(pushRequestObject.Id);

                        if (!pushRequestObject.DatabaseVersion.Equals(obj.Strategy.ObjectVersion))
                        {
                            this.AddVersionError(obj.Id);;
                        }
                        else if (this.AllowedClasses?.Contains(obj.Strategy.Class) == true)
                        {
                            this.PushRequestRoles(pushRequestObject, obj);
                        }
                        else
                        {
                            this.AddAccessError(pushRequestObject);
                        }
                    }
                }
            }

            var validation = this.Derive();
            if (validation.HasErrors)
            {
                this.AddDerivationErrors(validation.Errors);
            }

            if (!this.HasErrors)
            {

                this.Transaction.Commit();
            }
        }

        private void PushRequestRoles(LocalStrategy local, Database.IObject obj)
        {
            // TODO: Cache and filter for workspace
            var composite = (IComposite)obj.Strategy.Class;
            var roleTypes = composite.DatabaseRoleTypes.Where(v => v.RelationType.WorkspaceNames.Length > 0);
            var acl = this.AccessControlLists[obj];

            foreach (var keyValuePair in local.DatabaseState.ChangedRoleByRelationType)
            {
                var relationType = keyValuePair.Key;
                var roleType = ((IRelationType)this.M.FindByTag(keyValuePair.Key.Tag)).RoleType;

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
                    this.AddAccessError(local);
                }
            }
        }

        private Database.IObject GetRole(LocalStrategy localStrategy)
        {
            if (this.ObjectByNewId == null || !this.ObjectByNewId.TryGetValue(localStrategy.Id, out var role))
            {
                role = this.Transaction.Instantiate(localStrategy.Id);
            }

            return role;
        }

        private Database.IObject[] GetRoles(LocalStrategy[] localStrategies)
        {
            if (this.ObjectByNewId == null)
            {
                return this.Transaction.Instantiate(localStrategies.Select(v => v.Id));
            }

            var roles = new List<Database.IObject>();
            List<long> existingRoleIds = null;
            foreach (var localStrategy in localStrategies)
            {
                if (this.ObjectByNewId.TryGetValue(localStrategy.Id, out var role))
                {
                    roles.Add(role);
                }
                else
                {
                    existingRoleIds ??= new List<long>();
                    existingRoleIds.Add(localStrategy.Id);
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
