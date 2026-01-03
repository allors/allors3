// <copyright file="Database.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Allors.Database.Tracing;
using Meta;

public class Database : IDatabase
{
    private readonly ConcurrentDictionary<IObjectType, object> concreteClassesByObjectType;
    private readonly Store store;
    private readonly Lock globalLock;

    public Database(IDatabaseServices state, Configuration configuration)
    {
        this.Services = state;
        if (this.Services == null)
        {
            throw new Exception("Services is missing");
        }

        this.ObjectFactory = configuration.ObjectFactory;
        if (this.ObjectFactory == null)
        {
            throw new Exception("Configuration.ObjectFactory is missing");
        }

        this.MetaPopulation = this.ObjectFactory.MetaPopulation;

        this.concreteClassesByObjectType = new ConcurrentDictionary<IObjectType, object>();
        this.store = new Store();
        this.globalLock = new Lock();

        // Create SlotLayout for O(1) slot-based role/association access
        this.SlotLayout = new SlotLayout(this.MetaPopulation);

        this.Id = string.IsNullOrWhiteSpace(configuration.Id) ? Guid.NewGuid().ToString("N").ToLowerInvariant() : configuration.Id;

        this.Services.OnInit(this);
    }

    public event ObjectNotLoadedEventHandler ObjectNotLoaded;

    public event RelationNotLoadedEventHandler RelationNotLoaded;

    public string Id { get; }

    public bool IsShared => true;

    public IObjectFactory ObjectFactory { get; }

    public IMetaPopulation MetaPopulation { get; }

    public IDatabaseServices Services { get; }

    public ISink Sink { get; set; }

    internal bool IsLoading { get; private set; }

    internal Store Store => this.store;

    /// <summary>
    /// Pre-computed slot layout for O(1) role and association access.
    /// </summary>
    internal SlotLayout SlotLayout { get; }

    public ITransaction CreateTransaction() => this.CreateDatabaseTransaction();

    ITransaction IDatabase.CreateTransaction() => this.CreateDatabaseTransaction();

    public ITransaction CreateDatabaseTransaction() => new Transaction(this, this.Services.CreateTransactionServices());

    public void Load(XmlReader reader)
    {
        this.Init();

        try
        {
            this.IsLoading = true;

            using var transaction = (Transaction)this.CreateDatabaseTransaction();
            var load = new Load(transaction, reader);
            load.Execute();
            transaction.Commit();
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    public void Save(XmlWriter writer)
    {
        using var transaction = (Transaction)this.CreateDatabaseTransaction();
        transaction.Save(writer);
    }

    public bool ContainsClass(IComposite objectType, IObjectType concreteClass)
    {
        var concreteClassOrClasses = this.concreteClassesByObjectType.GetOrAdd(
            objectType,
            _ => objectType.ExistExclusiveDatabaseClass
                ? objectType.ExclusiveDatabaseClass
                : new HashSet<IObjectType>(objectType.DatabaseClasses));

        if (concreteClassOrClasses is IObjectType singleClass)
        {
            return concreteClass.Equals(singleClass);
        }

        var concreteClasses = (HashSet<IObjectType>)concreteClassOrClasses;
        return concreteClasses.Contains(concreteClass);
    }

    public void UnitRoleChecks(IStrategy strategy, IRoleType roleType)
    {
        if (!this.ContainsClass(roleType.AssociationType.ObjectType, strategy.Class))
        {
            throw new ArgumentException(strategy.Class + " is not a valid association object type for " + roleType + ".");
        }

        if (roleType.ObjectType is IComposite)
        {
            throw new ArgumentException(roleType.ObjectType + " on roleType " + roleType + " is not a unit type.");
        }
    }

    public void CompositeRoleChecks(IStrategy strategy, IRoleType roleType) => this.CompositeSharedChecks(strategy, roleType, null);

    public void CompositeRoleChecks(IStrategy strategy, IRoleType roleType, Strategy roleStrategy)
    {
        this.CompositeSharedChecks(strategy, roleType, roleStrategy);
        if (!roleType.IsOne)
        {
            throw new ArgumentException("RelationType " + roleType + " has multiplicity many.");
        }
    }

    public Strategy CompositeRolesChecks(IStrategy strategy, IRoleType roleType, Strategy roleStrategy)
    {
        this.CompositeSharedChecks(strategy, roleType, roleStrategy);
        if (!roleType.IsMany)
        {
            throw new ArgumentException("RelationType " + roleType + " has multiplicity one.");
        }

        return roleStrategy;
    }

    public virtual void Init()
    {
        using (this.globalLock.EnterScope())
        {
            this.store.Reset();
        }

        this.Services.OnInit(this);
    }

    internal long NextId() => this.store.NextId();

    internal void UpdateCurrentId(long objectId) => this.store.UpdateCurrentId(objectId);

    internal void CommitTransaction(TransactionChanges changes)
    {
        using (this.globalLock.EnterScope())
        {
            this.store.Commit(changes);
        }
    }

    internal void OnObjectNotLoaded(Guid metaTypeId, long allorsObjectId)
    {
        var args = new ObjectNotLoadedEventArgs(metaTypeId, allorsObjectId);
        if (this.ObjectNotLoaded != null)
        {
            this.ObjectNotLoaded(this, args);
        }
        else
        {
            throw new Exception("Object not loaded: " + args);
        }
    }

    internal void OnRelationNotLoaded(Guid relationTypeId, long associationObjectId, string roleContents)
    {
        var args = new RelationNotLoadedEventArgs(relationTypeId, associationObjectId, roleContents);
        if (this.RelationNotLoaded != null)
        {
            this.RelationNotLoaded(this, args);
        }
        else
        {
            throw new Exception("RelationType not loaded: " + args);
        }
    }

    private void CompositeSharedChecks(IStrategy strategy, IRoleType roleType, Strategy roleStrategy)
    {
        if (!this.ContainsClass(roleType.AssociationType.ObjectType, strategy.Class))
        {
            throw new ArgumentException(strategy.Class + " has no roleType with role " + roleType + ".");
        }

        if (roleStrategy != null)
        {
            if (!strategy.Transaction.Equals(roleStrategy.Transaction))
            {
                throw new ArgumentException(roleStrategy + " is from different transaction");
            }

            if (roleStrategy.IsDeleted)
            {
                throw new ArgumentException(roleType + " on object " + strategy + " is removed.");
            }

            if (!(roleType.ObjectType is IComposite compositeType))
            {
                throw new ArgumentException(roleStrategy + " has no CompositeType");
            }

            if (!compositeType.IsAssignableFrom(roleStrategy.Class))
            {
                throw new ArgumentException(roleStrategy.Class + " is not compatible with type " + roleType.ObjectType + " of role " + roleType + ".");
            }
        }
    }
}
