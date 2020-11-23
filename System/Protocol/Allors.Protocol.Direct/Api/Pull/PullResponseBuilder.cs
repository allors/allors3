// <copyright file="PullResponseBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Direct.Api.Pull
{
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Database.Data;

    public class PullResponseBuilder
    {
        private readonly Dictionary<string, ISet<IStrategy>> collectionsByName = new Dictionary<string, ISet<IStrategy>>();
        private readonly Dictionary<string, IStrategy> objectByName = new Dictionary<string, IStrategy>();
        private readonly Dictionary<string, string> valueByName = new Dictionary<string, string>();

        private readonly HashSet<IStrategy> objects;
        private Dictionary<IStrategy, AccessControl> accessControlByStrategy;

        public PullResponseBuilder(Api api)
        {
            this.Api = api;

            this.objects = new HashSet<IStrategy>();
            this.accessControlByStrategy = new Dictionary<IStrategy, AccessControl>();
        }

        public Api Api { get; }

        public void AddCollection(string name, in IEnumerable<IStrategy> collection)
        {
            switch (collection)
            {
                case ICollection<IStrategy> asCollection:
                    this.AddCollectionInternal(name, asCollection, null);
                    break;
                default:
                    this.AddCollectionInternal(name, collection.ToArray(), null);
                    break;
            }
        }

        public void AddCollection(string name, in ICollection<IStrategy> collection) => this.AddCollectionInternal(name, collection, null);

        public void AddCollection(string name, IEnumerable<IStrategy> collection, Node[] tree)
        {
            switch (collection)
            {
                case ICollection<IStrategy> list:
                    this.AddCollectionInternal(name, list, tree);
                    break;
                default:
                {
                    this.AddCollectionInternal(name, collection.ToArray(), tree);
                    break;
                }
            }
        }

        public void AddObject(string name, IStrategy @object)
        {
            if (@object != null)
            {
                this.AddObject(name, @object, null);
            }
        }

        public void AddObject(string name, IStrategy @object, Node[] tree)
        {
            // TODO:
            //if (@object != null)
            //{
            //    if (this.Api.AllowedClasses?.Contains(@object.Class) == true)
            //    {
            //        if (tree != null)
            //        {
            //            // Prefetch
            //            var session = @object.Session;
            //            var prefetcher = tree.BuildPrefetchPolicy();
            //            session.Prefetch(prefetcher, @object);
            //        }

            //        this.objects.Add(@object);
            //        this.objectByName[name] = @object;
            //        tree?.Resolve(@object, this.Api.AccessControlLists, this.objects);
            //    }
            //}
        }

        public void AddValue(string name, string value)
        {
            if (value != null)
            {
                this.valueByName.Add(name, value);
            }
        }

        private void AddCollectionInternal(string name, in ICollection<IStrategy> collection, Node[] tree)
        {
            // TODO:
            //if (collection?.Count > 0)
            //{
            //    this.collectionsByName.TryGetValue(name, out var existingCollection);

            //    var filteredCollection = collection.Where(v => this.Api.AllowedClasses != null && this.Api.AllowedClasses.Contains(v.Class));

            //    if (tree != null)
            //    {
            //        var prefetchPolicy = tree.BuildPrefetchPolicy();

            //        ICollection<IStrategy> newCollection;

            //        if (existingCollection != null)
            //        {
            //            newCollection = filteredCollection.ToArray();
            //            this.Api.Session.Prefetch(prefetchPolicy, newCollection);
            //            existingCollection.UnionWith(newCollection);
            //        }
            //        else
            //        {
            //            var newSet = new HashSet<IStrategy>(filteredCollection);
            //            newCollection = newSet;
            //            this.Api.Session.Prefetch(prefetchPolicy, newCollection);
            //            this.collectionsByName.Add(name, newSet);
            //        }

            //        this.objects.UnionWith(newCollection);

            //        foreach (var newObject in newCollection)
            //        {
            //            tree.Resolve(newObject, this.Api.AccessControlLists, this.objects);
            //        }
            //    }
            //    else
            //    {
            //        if (existingCollection != null)
            //        {
            //            existingCollection.UnionWith(filteredCollection);
            //        }
            //        else
            //        {
            //            var newWorkspaceCollection = new HashSet<IStrategy>(filteredCollection);
            //            this.collectionsByName.Add(name, newWorkspaceCollection);
            //            this.objects.UnionWith(newWorkspaceCollection);
            //        }
            //    }
            //}
        }

        public PullResponseBuilder With(Pull pull)
        {
            if (pull.Object != null)
            {
                var pullInstantiate = new PullInstantiate(pull);
                pullInstantiate.Execute(this);
            }
            else
            {
                var pullExtent = new PullExtent(pull);
                pullExtent.Execute(this);
            }

            return this;
        }

        public PullResponse Build()
        {
            var versionedIdByStrategy = this.Api.VersionedIdByStrategy;

            return null;

            // TODO:
            //return new PullResponse
            //{
            //    Objects = this.objects.Select(v => versionedIdByStrategy.Get(v.Strategy)).ToArray(),
            //    NamedObjects = this.objectByName.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Id),
            //    NamedCollections =
            //        this.collectionsByName.ToDictionary(kvp => kvp.Key,
            //            kvp => kvp.Value.Select(obj => obj.Id).ToArray()),
            //    NamedValues = this.valueByName,
            //    AccessControls = this.AccessControlLists.EffectivePermissionIdsByAccessControl.Keys
            //        .Select(v => new AccessControl(v.Strategy.ObjectId, v.Strategy.ObjectVersion,
            //            v.Permissions.Select(w => w.Id).ToArray()))
            //        .ToArray(),
            //};
        }
    }
}
