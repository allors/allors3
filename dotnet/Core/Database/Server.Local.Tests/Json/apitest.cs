// <copyright file="DomainTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Tests
{
    using System;
    using System.IO;
    using System.Reflection;
    using Allors.Database;
    using Allors.Database.Adapters.Sql;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Derivations;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Microsoft.Extensions.Configuration;
    using C1 = Allors.Database.Domain.C1;
    using Database = Allors.Database.Adapters.Sql.SqlClient.Database;
    using User = Allors.Database.Domain.User;

    public class ApiTest : IDisposable
    {
        public ApiTest(Fixture fixture, bool populate = true)
        {
            var configurationBuilder = new ConfigurationBuilder();

            const string root = "/config/core";
            configurationBuilder.AddCrossPlatform(".");
            configurationBuilder.AddCrossPlatform(root);
            configurationBuilder.AddCrossPlatform(Path.Combine(root, "commands"));
            configurationBuilder.AddEnvironmentVariables();

            var configuration = configurationBuilder.Build();

            var metaPopulation = new MetaBuilder().Build();
            var rules = Rules.Create(metaPopulation);
            var engine = new Engine(rules);
            var database = new Database(
                new DefaultDatabaseServices(engine),
                new Configuration
                {
                    ConnectionString = configuration["ConnectionStrings:DefaultConnection"],
                    ObjectFactory = new ObjectFactory(metaPopulation, typeof(C1)),
                });

            this.M = metaPopulation;

            this.Setup(database, populate);
        }

        public MetaPopulation M { get; }

        public virtual Config Config { get; } = new Config { SetupSecurity = true };

        public ITransaction Transaction { get; set; }

        public ITime Time => this.Transaction.Database.Services.Get<ITime>();

        public IDerivationService DerivationService => this.Transaction.Database.Services.Get<IDerivationService>();

        public ISecurity Security => this.Transaction.Database.Services.Get<ISecurity>();

        public TimeSpan? TimeShift
        {
            get => this.Time.Shift;

            set => this.Time.Shift = value;
        }

        public void Dispose()
        {
            this.Transaction.Rollback();
            this.Transaction = null;
        }

        protected void Setup(IDatabase database, bool populate)
        {
            database.Init();

            new Setup(database, this.Config).Apply();

            this.Transaction = database.CreateTransaction();

            if (populate)
            {
                this.Transaction.Commit();

                new TestPopulation(this.Transaction).Apply();
                this.Transaction.Commit();
            }
        }

        protected User SetUser(string userName) => this.Transaction.Services.Get<IUserService>().User = new Users(this.Transaction).FindBy(this.M.User.UserName, userName);

        protected Stream GetResource(string name)
        {
            var assembly = this.GetType().GetTypeInfo().Assembly;
            return assembly.GetManifestResourceStream(name);
        }

        protected byte[] GetResourceBytes(string name)
        {
            var assembly = this.GetType().GetTypeInfo().Assembly;
            var resource = assembly.GetManifestResourceStream(name);
            using var ms = new MemoryStream();
            resource?.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
