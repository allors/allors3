// <copyright file="PullResponseBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Pull;
    using Data;
    using Derivations;
    using Meta;
    using Ranges;
    using Security;
    using Services;

    public class PullResponseBuilder : IProcedureContext, IProcedureOutput
    {
        private readonly IUnitConvert unitConvert;
        private readonly IRanges<long> ranges;
        private readonly IDictionary<IClass, ISet<IPropertyType>> dependencies;

        private readonly Dictionary<string, ISet<IObject>> collectionsByName = new Dictionary<string, ISet<IObject>>();
        private readonly Dictionary<string, IObject> objectByName = new Dictionary<string, IObject>();
        private readonly Dictionary<string, object> valueByName = new Dictionary<string, object>();

        private readonly HashSet<IObject> maskedObjects;
        private readonly HashSet<IObject> unmaskedObjects;

        private List<IValidation> errors;
        private readonly IPullPrefetchers prefetchers;

        public PullResponseBuilder(ITransaction transaction, IAccessControl accessControl, ISet<IClass> allowedClasses, IPreparedSelects preparedSelects, IPreparedExtents preparedExtents, IUnitConvert unitConvert, IRanges<long> ranges, IDictionary<IClass, ISet<IPropertyType>> dependencies, IPullPrefetchers prefetchers)
        {
            this.unitConvert = unitConvert;
            this.ranges = ranges;
            this.dependencies = dependencies;
            this.Transaction = transaction;

            this.AccessControl = accessControl;
            this.AllowedClasses = allowedClasses;
            this.PreparedSelects = preparedSelects;
            this.PreparedExtents = preparedExtents;

            this.prefetchers = prefetchers;

            this.maskedObjects = new HashSet<IObject>();
            this.unmaskedObjects = new HashSet<IObject>();
        }

        public ITransaction Transaction { get; }

        public IAccessControl AccessControl { get; }

        public ISet<IClass> AllowedClasses { get; }

        public IPreparedSelects PreparedSelects { get; }

        public IPreparedExtents PreparedExtents { get; }

        public void AddError(IValidation validation)
        {
            this.errors ??= new List<IValidation>();
            this.errors.Add(validation);
        }

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

        public void AddValue(string name, object value)
        {
            if (value != null)
            {
                this.valueByName.Add(name, value);
            }
        }

        private void AddObjectInternal(string name, IObject @object, Node[] tree = null)
        {
            if (!this.Include(@object))
            {
                return;
            }

            var prefetchPolicy = this.prefetchers.ForInclude(@object.Strategy.Class, tree);
            var transaction = @object.Strategy.Transaction;
            transaction.Prefetch(prefetchPolicy, @object);

            this.unmaskedObjects.Add(@object);
            this.objectByName[name] = @object;
            tree?.Resolve(@object, this.AccessControl, this.Add);
        }

        private void AddCollectionInternal(string name, IComposite objectType, in IEnumerable<IObject> enumerable, Node[] tree)
        {
            var collection = enumerable as ICollection<IObject> ?? enumerable.ToArray();

            if (collection.Count > 0)
            {
                var prefetchPolicy = this.prefetchers.ForInclude(objectType, tree);

                this.collectionsByName.TryGetValue(name, out var existingCollection);

                var filteredCollection = collection.Where(this.Include);

                if (tree != null)
                {
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
                        this.collectionsByName.Add(name, newSet);
                    }

                    this.unmaskedObjects.UnionWith(newCollection);

                    foreach (var newObject in newCollection)
                    {
                        tree.Resolve(newObject, this.AccessControl, this.Add);
                    }
                }
                else
                {
                    this.Transaction.Prefetch(prefetchPolicy, filteredCollection);

                    if (existingCollection != null)
                    {
                        existingCollection.UnionWith(filteredCollection);
                    }
                    else
                    {
                        var newWorkspaceCollection = new HashSet<IObject>(filteredCollection);
                        this.collectionsByName.Add(name, newWorkspaceCollection);
                        this.unmaskedObjects.UnionWith(newWorkspaceCollection);
                    }
                }
            }
        }

        public PullResponse Build(PullRequest pullRequest = null)
        {
            var pullResponse = new PullResponse();

            var procedure = pullRequest?.p?.FromJson(this.Transaction, this.unitConvert);

            if (procedure != null)
            {
                if (procedure.Pool != null)
                {
                    foreach (var kvp in procedure.Pool)
                    {
                        var @object = kvp.Key;
                        var version = kvp.Value;

                        if (!@object.Strategy.ObjectVersion.Equals(version))
                        {
                            pullResponse.AddVersionError(@object);
                        }
                    }

                    if (pullResponse.HasErrors)
                    {
                        return pullResponse;
                    }
                }

                var procedures = this.Transaction.Database.Services.Get<IProcedures>();
                var proc = procedures.Get(procedure.Name);
                if (proc == null)
                {
                    pullResponse._e = $"Missing procedure {procedure.Name}";
                    return pullResponse;
                }

                var input = new ProcedureInput(this.Transaction.Database.ObjectFactory, procedure);

                proc.Execute(this, input, this);

                if (this.errors?.Count > 0)
                {
                    foreach (var error in this.errors)
                    {
                        pullResponse.AddDerivationErrors(error);
                    }

                    return pullResponse;
                }
            }

            if (pullRequest?.l != null)
            {
                var pulls = pullRequest.l.FromJson(this.Transaction, this.unitConvert);

                foreach (var pull in pulls)
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

            // Add dependencies
            this.AddDependencies();

            // Serialize
            var grants = new HashSet<IGrant>();
            var revocations = new HashSet<IRevocation>();

            pullResponse = new PullResponse
            {
                p = this.unmaskedObjects.Select(v =>
                {
                    var accessControlList = this.AccessControl[v];

                    grants.UnionWith(accessControlList.Grants);
                    revocations.UnionWith(accessControlList.Revocations);

                    return new PullResponseObject
                    {
                        i = v.Strategy.ObjectId,
                        v = v.Strategy.ObjectVersion,
                        g = this.ranges.Import(accessControlList.Grants.Select(w => w.Strategy.ObjectId)).Save(),
                        r = this.ranges.Import(accessControlList.Revocations.Select(w => w.Strategy.ObjectId))
                            .Save(),
                    };
                }).ToArray(),
                o = this.objectByName.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Id),
                c = this.collectionsByName.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(obj => obj.Id).ToArray()),
                v = this.valueByName,
            };

            pullResponse.g = grants.Count > 0 ? grants.Select(v => new[] { v.Strategy.ObjectId, v.Strategy.ObjectVersion }).ToArray() : null;
            pullResponse.r = revocations.Count > 0 ? revocations.Select(v => new[] { v.Strategy.ObjectId, v.Strategy.ObjectVersion }).ToArray() : null;

            return pullResponse;
        }

        public bool Include(IObject @object)
        {
            if (@object == null || this.AllowedClasses?.Contains(@object.Strategy.Class) != true ||
                this.maskedObjects.Contains(@object))
            {
                return false;
            }

            if (this.AccessControl[@object].IsMasked())
            {
                this.maskedObjects.Add(@object);
                return false;
            }

            return true;
        }

        private void AddDependencies()
        {
            if (this.dependencies == null)
            {
                return;
            }

            var current = this.unmaskedObjects.ToArray();

            while (current.Length > 0)
            {
                var newObjects = new HashSet<IObject>();

                foreach (var grouping in current.GroupBy(v => v.Strategy.Class, v => v))
                {
                    var @class = grouping.Key;
                    var grouped = grouping.ToArray();

                    if (this.dependencies.TryGetValue(@class, out var propertyTypes))
                    {
                        var prefetchPolicy = this.prefetchers.ForDependency(@class, propertyTypes);
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

                current = newObjects.Except(this.unmaskedObjects).ToArray();
                this.unmaskedObjects.UnionWith(newObjects);
            }
        }

        private void Add(IObject @object)
        {
            if (this.Include(@object))
            {
                this.unmaskedObjects.Add(@object);
            }
        }
    }
}
