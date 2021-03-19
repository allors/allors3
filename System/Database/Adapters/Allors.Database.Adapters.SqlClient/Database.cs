// <copyright file="Database.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.SqlClient
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using Microsoft.Data.SqlClient;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization;
    using Allors;
    using Allors.Database.Adapters.Schema;
    using Caching;
    using Meta;
    using Derivations;
    using Microsoft.Data.SqlClient.Server;

    public class Database : IDatabase
    {
        public static readonly IsolationLevel DefaultIsolationLevel = System.Data.IsolationLevel.Snapshot;

        private readonly object lockObject = new object();
        private readonly Dictionary<IObjectType, HashSet<IObjectType>> concreteClassesByObjectType;

        private readonly Dictionary<IObjectType, IRoleType[]> sortedUnitRolesByObjectType;

        private Mapping mapping;

        private bool? isValid;

        private string validationMessage;

        private IConnectionFactory connectionFactory;

        private IConnectionFactory managementConnectionFactory;

        private ICacheFactory cacheFactory;

        public Database(IDatabaseLifecycle state, Configuration configuration)
        {
            this.StateLifecycle = state;
            if (this.StateLifecycle == null)
            {
                throw new Exception("Services is missing");
            }

            this.ObjectFactory = configuration.ObjectFactory;
            if (!this.ObjectFactory.MetaPopulation.IsValid)
            {
                throw new ArgumentException("Domain is invalid");
            }

            this.ConnectionString = configuration.ConnectionString;
            this.ConnectionFactory = configuration.ConnectionFactory;
            this.ManagementConnectionFactory = configuration.ManagementConnectionFactory;

            this.concreteClassesByObjectType = new Dictionary<IObjectType, HashSet<IObjectType>>();

            this.CommandTimeout = configuration.CommandTimeout;
            this.IsolationLevel = configuration.IsolationLevel;

            this.sortedUnitRolesByObjectType = new Dictionary<IObjectType, IRoleType[]>();

            this.CacheFactory = configuration.CacheFactory;
            this.Cache = this.CacheFactory.CreateCache();

            var connectionStringBuilder = new SqlConnectionStringBuilder(this.ConnectionString);
            var applicationName = connectionStringBuilder.ApplicationName.Trim();
            if (!string.IsNullOrWhiteSpace(applicationName))
            {
                this.Id = applicationName;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(connectionStringBuilder.InitialCatalog))
                {
                    this.Id = connectionStringBuilder.InitialCatalog.ToLowerInvariant();
                }
                else
                {
                    using var connection = new SqlConnection(this.ConnectionString);
                    connection.Open();
                    this.Id = connection.Database.ToLowerInvariant();
                }
            }

            this.SchemaName = (configuration.SchemaName ?? "allors").ToLowerInvariant();

            this.Derivations = Array.Empty<IDomainDerivation>();
            this.Procedures = new DefaultProcedures(this.ObjectFactory.Assembly);

            this.StateLifecycle.OnInit(this);
        }

        public event ObjectNotLoadedEventHandler ObjectNotLoaded;

        public event RelationNotLoadedEventHandler RelationNotLoaded;

        public IDomainDerivation[] Derivations { get; private set; }

        public IProcedures Procedures { get; }

        public IDatabaseLifecycle StateLifecycle { get; }

        public IConnectionFactory ConnectionFactory
        {
            get => this.connectionFactory ??= new DefaultConnectionFactory();

            set => this.connectionFactory = value;
        }

        public IConnectionFactory ManagementConnectionFactory
        {
            get => this.managementConnectionFactory ??= new DefaultConnectionFactory();

            set => this.managementConnectionFactory = value;
        }

        public ICacheFactory CacheFactory
        {
            get => this.cacheFactory;

            set => this.cacheFactory = value ?? (this.cacheFactory = new DefaultCacheFactory());
        }

        public string Id { get; }

        public string SchemaName { get; }

        public IObjectFactory ObjectFactory { get; }

        public IMetaPopulation MetaPopulation => this.ObjectFactory.MetaPopulation;

        public bool IsShared => true;

        public bool IsValid
        {
            get
            {
                if (!this.isValid.HasValue)
                {
                    lock (this.lockObject)
                    {
                        if (!this.isValid.HasValue)
                        {
                            var validate = this.Validate();
                            this.validationMessage = validate.Message;
                            return validate.IsValid;
                        }
                    }
                }

                return this.isValid.Value;
            }
        }

        internal string ConnectionString { get; set; }

        internal string AscendingSortAppendix => null;

        internal string DescendingSortAppendix => null;

        internal string Except => "EXCEPT";

        internal ICache Cache { get; }

        internal int? CommandTimeout { get; }

        internal IsolationLevel? IsolationLevel { get; }

        internal Mapping Mapping
        {
            get
            {
                if (this.ObjectFactory.MetaPopulation != null)
                {
                    if (this.mapping == null)
                    {
                        this.mapping = new Mapping(this);
                    }
                }

                return this.mapping;
            }
        }

        public ITransaction CreateTransaction()
        {
            var connection = this.ConnectionFactory.Create(this);
            return this.CreateTransaction(connection);
        }

        public ITransaction CreateTransaction(Connection connection)
        {
            if (!this.IsValid)
            {
                throw new Exception(this.validationMessage);
            }

            return new Transaction(this, connection, this.StateLifecycle.CreateTransactionInstance());
        }

        public void AddDerivation(IDomainDerivation derivation) => this.Derivations = new List<IDomainDerivation>(this.Derivations) { derivation }.ToArray();

        public void Init()
        {
            try
            {
                new Initialization(this).Execute();
            }
            finally
            {
                this.ResetSchema();
                this.Cache.Invalidate();
                this.StateLifecycle.OnInit(this);
            }
        }

        public void Load1(XmlReader reader)
        {
            lock (this)
            {
                var xmlSerializer = new XmlSerializer(typeof(Xml));
                var xml = (Xml)xmlSerializer.Deserialize(reader);
                var load = new Load(this, this.ObjectNotLoaded, this.RelationNotLoaded, xml);
                load.Execute();
            }
        }

        public void Load(XmlReader reader)
        {
            lock (this)
            {
                this.Init();

                using (var connection = new SqlConnection(this.ConnectionString))
                {
                    try
                    {
                        connection.Open();

                        var load = new Load2(this, connection, this.ObjectNotLoaded, this.RelationNotLoaded);
                        load.Execute(reader);

                        connection.Close();
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            connection.Close();
                        }
                        finally
                        {
                            this.Init();
                            throw e;
                        }
                    }
                }
            }
        }

        public void Save(XmlWriter writer)
        {
            lock (this)
            {
                var transaction = new ManagementTransaction(this, this.ManagementConnectionFactory);
                try
                {
                    var save = new Save(this, writer);
                    save.Execute(transaction);
                }
                finally
                {
                    transaction.Rollback();
                }
            }
        }

        public override string ToString() => "Population[driver=Sql, type=Connected, id=" + this.GetHashCode() + "]";

        public Validation Validate()
        {
            var validateResult = new Validation(this);
            this.isValid = validateResult.IsValid;
            return validateResult;
        }

        ITransaction IDatabase.CreateTransaction() => this.CreateTransaction();

        internal bool ContainsClass(IObjectType container, IObjectType containee)
        {
            if (container.IsClass)
            {
                return container.Equals(containee);
            }

            if (!this.concreteClassesByObjectType.TryGetValue(container, out var concreteClasses))
            {
                concreteClasses = new HashSet<IObjectType>(((IInterface)container).DatabaseClasses);
                this.concreteClassesByObjectType[container] = concreteClasses;
            }

            return concreteClasses.Contains(containee);
        }

        internal IEnumerable<SqlDataRecord> CreateObjectTable(IEnumerable<long> objectids) => new ObjectDataRecord(this.mapping, objectids);

        internal IEnumerable<SqlDataRecord> CreateVersionedObjectTable(Dictionary<long, long> versionedObjects) => new VersionedObjectDataRecord(this.mapping, versionedObjects);

        internal IEnumerable<SqlDataRecord> CreateObjectTable(IEnumerable<Reference> references) => new CompositesRoleDataRecords(this.mapping, references);

        internal IEnumerable<SqlDataRecord> CreateCompositeRelationTable(IEnumerable<CompositeRelation> relations) => new CompositeRoleDataRecords(this.mapping, relations);

        internal IEnumerable<SqlDataRecord> CreateUnitRelationTable(IRoleType roleType, IEnumerable<UnitRelation> relations) => new UnitRoleDataRecords(this, roleType, relations);

        internal Type GetDomainType(IObjectType objectType) => this.ObjectFactory.GetType(objectType);

        internal IRoleType[] GetSortedUnitRolesByObjectType(IObjectType objectType)
        {
            if (!this.sortedUnitRolesByObjectType.TryGetValue(objectType, out var sortedUnitRoles))
            {
                var sortedUnitRoleList = new List<IRoleType>(((IComposite)objectType).DatabaseRoleTypes.Where(r => r.ObjectType.IsUnit));
                sortedUnitRoleList.Sort();
                sortedUnitRoles = sortedUnitRoleList.ToArray();
                this.sortedUnitRolesByObjectType[objectType] = sortedUnitRoles;
            }

            return sortedUnitRoles;
        }

        // TODO: inline
        internal SqlMetaData GetSqlMetaData(string name, IRoleType roleType)
        {
            var unit = (IUnit)roleType.ObjectType;
            switch (unit.UnitTag)
            {
                case UnitTags.String:
                    if (roleType.Size == -1 || roleType.Size > 4000)
                    {
                        return new SqlMetaData(name, SqlDbType.NVarChar, -1);
                    }

                    return new SqlMetaData(name, SqlDbType.NVarChar, roleType.Size.Value);

                case UnitTags.Integer:
                    return new SqlMetaData(name, SqlDbType.Int);

                case UnitTags.Decimal:
                    return new SqlMetaData(name, SqlDbType.Decimal, (byte)roleType.Precision.Value, (byte)roleType.Scale.Value);

                case UnitTags.Float:
                    return new SqlMetaData(name, SqlDbType.Float);

                case UnitTags.Boolean:
                    return new SqlMetaData(name, SqlDbType.Bit);

                case UnitTags.DateTime:
                    return new SqlMetaData(name, SqlDbType.DateTime2);

                case UnitTags.Unique:
                    return new SqlMetaData(name, SqlDbType.UniqueIdentifier);

                case UnitTags.Binary:
                    if (roleType.Size == -1 || roleType.Size > 8000)
                    {
                        return new SqlMetaData(name, SqlDbType.VarBinary, -1);
                    }

                    return new SqlMetaData(name, SqlDbType.VarBinary, (long)roleType.Size);

                default:
                    throw new Exception("!UNKNOWN VALUE TYPE!");
            }
        }

        private void ResetSchema() => this.mapping = null;
    }
}
