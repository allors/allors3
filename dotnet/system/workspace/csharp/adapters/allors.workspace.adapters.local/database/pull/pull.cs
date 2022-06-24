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
    using Database.Domain;
    using Database.Security;
    using Database.Services;
    using Meta;
    using Protocol.Direct;
    using IClass = Database.Meta.IClass;
    using IComposite = Database.Meta.IComposite;
    using IObject = IObject;
    using IPropertyType = Database.Meta.IPropertyType;
    using IRelationType = Database.Meta.IRelationType;

    public class Pull : Result, IPullResultInternals, IProcedureOutput
    {
        private IDictionary<string, IObject[]> collections;
        private IDictionary<string, IObject> objects;
        private readonly IDictionary<IClass, ISet<IPropertyType>> dependencies;

        public Pull(Session session) : base(session)
        {
            this.Workspace = session.Workspace;
            var database = this.Workspace.DatabaseConnection.Database;
            this.Transaction = database.CreateTransaction();

            this.AllowedClasses = database.Services.Get<IMetaCache>()
                .GetWorkspaceClasses(this.Workspace.DatabaseConnection.Configuration.Name);
            this.PreparedSelects = database.Services.Get<IPreparedSelects>();
            this.PreparedExtents = database.Services.Get<IPreparedExtents>();
            this.PrefetchPolicyCache = database.Services.Get<IPrefetchPolicyCache>();

            this.AccessControl = this.Transaction.Services.Get<IWorkspaceAclsService>()
                .Create(this.Workspace.DatabaseConnection.Configuration.Name);

            this.dependencies = this.ToDependencies(session.Dependencies);
            this.DatabaseObjects = new HashSet<Database.IObject>();
        }

        public IAccessControl AccessControl { get; }

        public HashSet<Database.IObject> DatabaseObjects { get; }

        private Dictionary<string, ISet<Database.IObject>> DatabaseCollectionsByName { get; } =
            new Dictionary<string, ISet<Database.IObject>>();

        private Dictionary<string, Database.IObject> DatabaseObjectByName { get; } =
            new Dictionary<string, Database.IObject>();

        private Dictionary<string, object> ValueByName { get; } = new Dictionary<string, object>();

        private Workspace Workspace { get; }

        private ITransaction Transaction { get; }

        private ISet<IClass> AllowedClasses { get; }

        private IPreparedSelects PreparedSelects { get; }

        private IPreparedExtents PreparedExtents { get; }

        public IPrefetchPolicyCache PrefetchPolicyCache { get; }

        public void AddCollection(string name, IComposite objectType, in IEnumerable<Database.IObject> collection, Node[] tree)
        {
            switch (collection)
            {
                case ICollection<Database.IObject> list:
                    this.AddCollectionInternal(name, list, tree);
                    break;
                default:
                    this.AddCollectionInternal(name, collection.ToArray(), tree);
                    break;
            }
        }

        public void AddObject(string name, Database.IObject @object, Node[] tree)
        {
            this.AddObjectInternal(name, @object, tree);
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
                v => v.Value.Select(w => this.Session.Instantiate<IObject>(w.Id)).ToArray());

        public IDictionary<string, IObject> Objects =>
            this.objects ??= this.DatabaseObjectByName.ToDictionary(v => v.Key,
                v => this.Session.Instantiate<IObject>(v.Value.Id));

        public IDictionary<string, object> Values => this.ValueByName;

        public T[] GetCollection<T>() where T : class, IObject
        {
            var objectType = this.Workspace.DatabaseConnection.Configuration.ObjectFactory.GetObjectType<T>();
            var key = objectType.PluralName;
            return this.GetCollection<T>(key);
        }

        public T[] GetCollection<T>(string key) where T : class, IObject =>
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

        public void Execute(object args, string name)
        {
            // TODO: Use a Service for raw procedures
            throw new NotImplementedException();
        }

        public void Execute(Data.Procedure workspaceProcedure)
        {
            var visitor = new ToDatabaseVisitor(this.Transaction);
            var procedure = visitor.Visit(workspaceProcedure);
            var localProcedure = new Procedure(this.Transaction, procedure, this.AccessControl);
            localProcedure.Execute(this);
        }

        public void Execute(IEnumerable<Data.Pull> workspacePulls)
        {
            var visitor = new ToDatabaseVisitor(this.Transaction);
            foreach (var pull in workspacePulls.Select(v => visitor.Visit(v)))
            {
                if (pull.Object != null)
                {
                    var pullInstantiate = new PullInstantiate(this.Transaction, pull, this.AccessControl, this.PreparedSelects);
                    pullInstantiate.Execute(this);
                }
                else
                {
                    var pullExtent = new PullExtent(this.Transaction, pull, this.AccessControl, this.PreparedSelects, this.PreparedExtents);
                    pullExtent.Execute(this);
                }
            }
        }

        public void AddCollection(string name, in ICollection<Database.IObject> collection) => this.AddCollectionInternal(name, collection, null);

        public void AddObject(string name, Database.IObject @object) => this.AddObjectInternal(name, @object, null);

        private void AddObjectInternal(string name, Database.IObject @object, Node[] tree)
        {
            if (@object == null || this.AllowedClasses?.Contains(@object.Strategy.Class) != true || this.AccessControl[@object].IsMasked())
            {
                return;
            }


            this.DatabaseObjects.Add(@object);
            this.DatabaseObjectByName[name] = @object;
            tree?.Resolve(@object, this.AccessControl, this.Add, this.PrefetchPolicyCache, this.Transaction);
        }

        private void AddCollectionInternal(string name, in ICollection<Database.IObject> collection, Node[] tree)
        {
            if (collection?.Count > 0)
            {
                this.DatabaseCollectionsByName.TryGetValue(name, out var existingCollection);

                var filteredCollection = collection.Where(v =>
                    this.AllowedClasses != null && this.AllowedClasses.Contains(v.Strategy.Class) && !this.AccessControl[v].IsMasked());

                if (tree != null)
                {
                    // TODO: 
                    //var prefetchPolicy = tree.BuildPrefetchPolicy();

                    ICollection<Database.IObject> newCollection;

                    if (existingCollection != null)
                    {
                        newCollection = filteredCollection.ToArray();
                        //this.Transaction.Prefetch(prefetchPolicy, newCollection);
                        existingCollection.UnionWith(newCollection);
                    }
                    else
                    {
                        var newSet = new HashSet<Database.IObject>(filteredCollection);
                        newCollection = newSet;
                        //this.Transaction.Prefetch(prefetchPolicy, newCollection);
                        this.DatabaseCollectionsByName.Add(name, newSet);
                    }

                    this.DatabaseObjects.UnionWith(newCollection);

                    tree.Resolve(newCollection, this.AccessControl, this.Add, this.PrefetchPolicyCache, this.Transaction);
                }
                else if (existingCollection != null)
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

        private IDictionary<IClass, ISet<IPropertyType>> ToDependencies(ISet<IDependency> pullDependencies)
        {
            var classDependencies = new Dictionary<IClass, ISet<IPropertyType>>();

            var m = this.Workspace.DatabaseConnection.Database.MetaPopulation;
            foreach (var pullDependency in pullDependencies)
            {
                var objectType = (IComposite)m.FindByTag(pullDependency.ObjectType.Tag);
                IPropertyType propertyType;
                if (pullDependency.PropertyType is IAssociationType associationType)
                {
                    propertyType = ((IRelationType)m.FindByTag(associationType.RelationType.Tag)).AssociationType;
                }
                else
                {
                    var roleType = (IRoleType)pullDependency.PropertyType;
                    propertyType = ((IRelationType)m.FindByTag(roleType.RelationType.Tag)).RoleType;
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

        public void AddDependencies()
        {
            if (this.dependencies == null)
            {
                return;
            }

            var current = this.DatabaseObjects.ToArray();

            while (current.Length > 0)
            {
                foreach (var grouping in current.GroupBy(v => v.Strategy.Class, v => v))
                {
                    var @class = grouping.Key;
                    var objects = grouping.ToArray();

                    if (this.dependencies.TryGetValue(@class, out var propertyTypes))
                    {
                        var builder = new PrefetchPolicyBuilder();
                        foreach (var propertyType in propertyTypes)
                        {
                            builder.WithRule(propertyType);
                        }

                        var policy = builder.Build();

                        this.Transaction.Prefetch(policy, objects);

                        foreach (var objectToAdd in objects)
                        {
                            foreach (var propertyType in propertyTypes)
                            {
                                if (propertyType is Database.Meta.IRoleType roleType)
                                {
                                    if (roleType.IsOne)
                                    {
                                        this.DatabaseObjects.Add(objectToAdd.Strategy.GetCompositeRole(roleType));
                                    }
                                    else
                                    {
                                        this.DatabaseObjects.UnionWith(objectToAdd.Strategy.GetCompositesRole<Database.IObject>(roleType));
                                    }
                                }
                                else
                                {
                                    var associationType = (Database.Meta.IAssociationType)propertyType;
                                    if (associationType.IsOne)
                                    {
                                        this.DatabaseObjects.Add(objectToAdd.Strategy.GetCompositeAssociation(associationType));
                                    }
                                    else
                                    {
                                        this.DatabaseObjects.UnionWith(objectToAdd.Strategy.GetCompositesAssociation<Database.IObject>(associationType));
                                    }
                                }
                            }
                        }
                    }
                }

                current = this.DatabaseObjects.Except(current).ToArray();
            }
        }

        private void Add(Database.IObject @object)
        {
            if (this.AccessControl[@object].IsMasked())
            {
                return;
            }

            this.DatabaseObjects.Add(@object);
        }
    }
}
