// <copyright file="DomainTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Tests
{
    using System;
    using System.IO;
    using System.Reflection;
    using Allors.Database;
    using Allors.Database.Configuration;
    using Allors.Database.Derivations;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Allors.Database.Security;
    using Allors.Database.Services;

    public class ApiTest : IDisposable
    {
        private readonly AllorsWebApplicationFactory factory;

        public ApiTest(Fixture fixture, bool populate = true)
        {
            this.factory = new AllorsWebApplicationFactory();

            // Force the factory to create the host, which triggers Startup.Configure
            // and builds the database via DatabaseBuilder with MEMORY adapter
            _ = this.factory.Server;

            var database = this.factory.Database;
            this.M = (MetaPopulation)database.MetaPopulation;

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
            this.factory.Dispose();
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

        protected IUser SetUser(string userName) => this.Transaction.Services.Get<IUserService>().User = new Users(this.Transaction).FindBy(this.M.User.UserName, userName);

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
