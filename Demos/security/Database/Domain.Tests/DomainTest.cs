// <copyright file="DomainTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.IO;
    using System.Reflection;
    using Allors;
    using Allors.Database.Adapters.Memory;
    using Allors.Database.Domain;
    using Configuration;
    using Meta;
    using User = Domain.User;

    public class DomainTest : IDisposable
    {
        public DomainTest(Fixture fixture, bool populate = true)
        {
            var database = new Database(
                new DefaultDatabaseContext(fixture.Engine),
                new Configuration
                {
                    M = fixture.M,
                    ObjectFactory = new ObjectFactory(fixture.MetaPopulation, typeof(User)),
                });

            this.M = database.Context().M;

            this.Setup(database, populate);
        }

        public M M { get; }

        public virtual Config Config { get; } = new Config { SetupSecurity = false };

        public ITransaction Session { get; private set; }

        public ITime Time => this.Session.Database.Context().Time;

        public IDerivationFactory DerivationFactory => this.Session.Database.Context().DerivationFactory;

        public TimeSpan? TimeShift
        {
            get => this.Time.Shift;

            set => this.Time.Shift = value;
        }

        public void Dispose()
        {
            this.Session.Rollback();
            this.Session = null;
        }

        protected void Setup(IDatabase database, bool populate)
        {
            database.Init();
            this.Session = database.CreateTransaction();

            if (populate)
            {
                new Setup(this.Session, this.Config).Apply();
                this.Session.Commit();
            }
        }

        protected Stream GetResource(string name)
        {
            var assembly = this.GetType().GetTypeInfo().Assembly;
            var resource = assembly.GetManifestResourceStream(name);
            return resource;
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
