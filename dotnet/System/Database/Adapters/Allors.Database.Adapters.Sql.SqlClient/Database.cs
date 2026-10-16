// <copyright file="Database.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using System.Xml;
    using Microsoft.Data.SqlClient;

    public class Database : Sql.Database
    {
        private IConnectionFactory connectionFactory;

        private bool hasCustomConnectionFactory;

        private IConnectionFactory managementConnectionFactory;

        private Mapping mapping;

        private bool? isValid;

        private string validationMessage;

        private readonly object lockObject = new object();

        public Database(IDatabaseServices state, Configuration configuration) : base(state, configuration)
        {
            if (configuration.IsolationLevel.HasValue && configuration.IsolationLevel.Value != System.Data.IsolationLevel.Snapshot)
            {
                throw new ArgumentException("SQL Server requires Snapshot isolation. Leave IsolationLevel unset or set it to Snapshot.", nameof(configuration));
            }

            this.connectionFactory = configuration.ConnectionFactory;
            this.hasCustomConnectionFactory = configuration.ConnectionFactory != null;
            this.managementConnectionFactory = configuration.ManagementConnectionFactory;
            this.PopulationLifecycle = new PopulationLifecycle(new PopulationMetadataStore(this));

            var connectionStringBuilder = new SqlConnectionStringBuilder(this.ConnectionString);
            var applicationName = connectionStringBuilder.ApplicationName.Trim();
            if (!string.IsNullOrWhiteSpace(applicationName))
            {
                this.Id = applicationName;
            }
            else if (!string.IsNullOrWhiteSpace(connectionStringBuilder.InitialCatalog))
            {
                this.Id = connectionStringBuilder.InitialCatalog.ToLowerInvariant();
            }
            else
            {
                using var connection = new SqlConnection(this.ConnectionString);
                connection.Open();
                this.Id = connection.Database.ToLowerInvariant();
            }

            this.Services.OnInit(this);
        }

        public override event ObjectNotLoadedEventHandler ObjectNotLoaded;

        public override event RelationNotLoadedEventHandler RelationNotLoaded;

        public override IConnectionFactory ConnectionFactory
        {
            get => this.connectionFactory ??= new ConnectionFactory(this);

            set
            {
                this.connectionFactory = value;
                this.hasCustomConnectionFactory = value != null;
            }
        }

        public override IConnectionFactory ManagementConnectionFactory
        {
            get => this.managementConnectionFactory ??= new ConnectionFactory(this);

            set => this.managementConnectionFactory = value;
        }

        public override string Id { get; }

        public override bool IsValid
        {
            get
            {
                if (!this.isValid.HasValue)
                {
                    lock (this.lockObject)
                    {
                        if (!this.isValid.HasValue)
                        {
                            var validateResult = new Validation(this);
                            this.isValid = validateResult.IsValid;
                            this.validationMessage = validateResult.Message;
                        }
                    }
                }

                return this.isValid.Value;
            }
        }

        public override Sql.Mapping Mapping
        {
            get
            {
                if (this.ObjectFactory.MetaPopulation != null && this.mapping == null)
                {
                    this.mapping = new Mapping(this);
                }

                return this.mapping;
            }
        }

        public override string ValidationMessage => this.validationMessage;

        public override WriteScope OpenWrite(Guid expectedIncarnation)
        {
            if (this.hasCustomConnectionFactory)
            {
                throw new InvalidOperationException("Protected writes require the built-in connection factory. Use a database configured without a custom ConnectionFactory.");
            }

            if (!this.IsValid)
            {
                throw new InvalidOperationException("Cannot open a protected write on an invalid schema. " + this.ValidationMessage);
            }

            var connection = new Connection(this);
            PopulationMetadata population;
            try
            {
                population = connection.OpenWrite();
            }
            catch
            {
                connection.Rollback();
                throw;
            }

            return this.CreateWriteScope(connection, population, expectedIncarnation, connection.Commit);
        }

        public override ReadScope OpenRead()
        {
            if (this.hasCustomConnectionFactory)
            {
                throw new InvalidOperationException("Protected reads require the built-in connection factory. Use a database configured without a custom ConnectionFactory.");
            }

            if (!this.IsValid)
            {
                throw new InvalidOperationException("Cannot open a protected read on an invalid schema. " + this.ValidationMessage);
            }

            var connection = new Connection(this);
            PopulationMetadata population;
            Action verifySnapshot;
            try
            {
                population = connection.OpenRead(out verifySnapshot);
            }
            catch
            {
                connection.Rollback();
                throw;
            }

            return this.CreateReadScope(connection, population, verifySnapshot);
        }

        public override void Init()
        {
            using var operation = this.PopulationLifecycle.BeginOperation();
            try
            {
                new Initialization(this).Execute();
            }
            finally
            {
                this.mapping = null;
                this.Cache.Invalidate();
                this.Services.OnInit(this);
            }

            operation.Complete();
        }

        public override void Load(XmlReader reader)
        {
            using var operation = this.PopulationLifecycle.BeginOperation();
            lock (this)
            {
                this.Init();

                using (var connection = new SqlConnection(this.ConnectionString))
                {
                    try
                    {
                        connection.Open();

                        var load = new Load(this, connection, this.ObjectNotLoaded, this.RelationNotLoaded);
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

            operation.Complete();
        }

        public override void Save(XmlWriter writer)
        {
            lock (this.lockObject)
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
    }
}
