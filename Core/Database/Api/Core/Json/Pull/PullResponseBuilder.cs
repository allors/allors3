// <copyright file="PullResponseBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Api.Json.Pull
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Data;
    using Allors.Domain;
    using Allors.Protocol.Remote.Pull;
    using Protocol;
    using Allors.Services;
    using Server;

    public class PullResponseBuilder
    {
        private readonly Dictionary<string, ISet<IObject>> collectionsByName = new Dictionary<string, ISet<IObject>>();
        private readonly Dictionary<string, IObject> objectByName = new Dictionary<string, IObject>();
        private readonly Dictionary<string, object> valueByName = new Dictionary<string, object>();

        private readonly HashSet<IObject> objects;

        private readonly AccessControlsWriter accessControlsWriter;
        private readonly PermissionsWriter permissionsWriter;

        public PullResponseBuilder(ISession session, string workspaceName)
        {
            this.Session = session;
            var sessionState = session.State();
            var databaseState = session.Database.State();

            this.AccessControlLists = new WorkspaceAccessControlLists(workspaceName, sessionState.User);
            this.TreeCache = databaseState.TreeCache;
            this.WorkspaceMeta = databaseState.WorkspaceMetaCache.Get(workspaceName);

            this.objects = new HashSet<IObject>();
            this.accessControlsWriter = new AccessControlsWriter(this.AccessControlLists);
            this.permissionsWriter = new PermissionsWriter(this.AccessControlLists);
        }

        public ISession Session { get; }

        public IAccessControlLists AccessControlLists { get; }

        public IWorkspaceMetaCacheEntry WorkspaceMeta { get; }

        public ITreeCache TreeCache { get; set; }

        public void AddCollection(string name, in IEnumerable<IObject> collection, bool full = false)
        {
            switch (collection)
            {
                case ICollection<IObject> asCollection:
                    this.AddCollection(name, asCollection, null, full);
                    break;
                default:
                    this.AddCollection(name, collection.ToArray(), null, full);
                    break;
            }
        }

        public void AddCollection(string name, in ICollection<IObject> collection, bool full = false) => this.AddCollection(name, collection, null, full);

        public void AddCollection(string name, IEnumerable<IObject> collection, Node[] tree)
        {
            switch (collection)
            {
                case ICollection<IObject> list:
                    this.AddCollection(name, list, tree, false);
                    break;
                default:
                {
                    this.AddCollection(name, collection.ToArray(), tree, false);
                    break;
                }
            }
        }

        public void AddCollection(string name, in ICollection<IObject> collection, Node[] tree) => this.AddCollection(name, collection, tree, false);

        public void AddObject(string name, IObject @object, bool full = false)
        {
            if (@object != null)
            {
                Node[] tree = null;
                if (full)
                {
                    tree = @object.Strategy.Session.Database.FullTree(@object.Strategy.Class, this.TreeCache);
                }

                this.AddObject(name, @object, tree);
            }
        }

        public void AddObject(string name, IObject @object, Node[] tree)
        {
            if (this.WorkspaceMeta != null && @object != null)
            {
                var classes = this.WorkspaceMeta.Classes;
                if ( classes.Contains(@object.Strategy.Class))
                {

                    if (tree != null)
                    {
                        // Prefetch
                        var session = @object.Strategy.Session;
                        var prefetcher = tree.BuildPrefetchPolicy();
                        session.Prefetch(prefetcher, @object);
                    }

                    this.objects.Add(@object);
                    this.objectByName[name] = @object;
                    tree?.Resolve(@object, this.AccessControlLists, this.objects);
                }
            }
        }

        public void AddValue(string name, object value)
        {
            if (value != null)
            {
                this.valueByName.Add(name, value);
            }
        }

        private void AddCollection(string name, in ICollection<IObject> collection, Node[] tree, bool full)
        {
            if (this.WorkspaceMeta != null && collection.Count > 0)
            {
                if (full)
                {
                    if (tree != null)
                    {
                        throw new ArgumentException("tree must be null when full is true");
                    }

                    var @object = collection.First();
                    tree = @object?.Strategy.Session.Database.FullTree(@object.Strategy.Class, this.TreeCache);
                }

                var classes = this.WorkspaceMeta.Classes;
                this.collectionsByName.TryGetValue(name, out var existingCollection);

                var filteredCollection = collection.Where(v => classes.Contains(v.Strategy.Class));

                if (tree != null)
                {
                    var prefetchPolicy = tree.BuildPrefetchPolicy();

                    ICollection<IObject> newCollection;

                    if (existingCollection != null)
                    {
                        newCollection = filteredCollection.ToArray();
                        this.Session.Prefetch(prefetchPolicy, newCollection);
                        existingCollection.UnionWith(newCollection);
                    }
                    else
                    {
                        var newSet = new HashSet<IObject>(filteredCollection);
                        newCollection = newSet;
                        this.Session.Prefetch(prefetchPolicy, newCollection);
                        this.collectionsByName.Add(name, newSet);
                    }

                    this.objects.UnionWith(newCollection);

                    foreach (var newObject in newCollection)
                    {
                        tree.Resolve(newObject, this.AccessControlLists, this.objects);
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
                        this.collectionsByName.Add(name, newWorkspaceCollection);
                        this.objects.UnionWith(newWorkspaceCollection);
                    }
                }
            }
        }

        public PullResponse Build()
        {
            var pullResponse = new PullResponse
            {
                Objects = this.objects.Select(v =>
                {
                    var strategy = v.Strategy;
                    var id = strategy.ObjectId.ToString();
                    var version = strategy.ObjectVersion.ToString();
                    var accessControls = this.accessControlsWriter.Write(v);
                    var deniedPermissions = this.permissionsWriter.Write(v);
                    return deniedPermissions != null
                        ? new[] { id, version, accessControls, deniedPermissions }
                        : new[] { id, version, accessControls };
                }).ToArray(),
                NamedObjects = this.objectByName.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Id.ToString()),
                NamedCollections = this.collectionsByName.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(obj => obj.Id.ToString()).ToArray()),
                NamedValues = this.valueByName,
            };

            pullResponse.AccessControls = this.AccessControlLists.EffectivePermissionIdsByAccessControl.Keys
                .Select(v => new[]
                {
                    v.Strategy.ObjectId.ToString(),
                    v.Strategy.ObjectVersion.ToString(),
                })
                .ToArray();

            return pullResponse;
        }
    }
}
