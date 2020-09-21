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
    using Allors;
    using Allors.Database.Adapters.Memory;
    using Allors.Domain;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Allors.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using ObjectFactory = Allors.ObjectFactory;

    public class DomainTest : IDisposable
    {
        public DomainTest(bool populate = true)
        {
            this.MetaPopulation = new MetaBuilder().Build();
            this.M = new M(this.MetaPopulation);
            var database = new Database(
                new DefaultDatabaseScope(), 
                new Configuration
                {
                    ObjectFactory = new ObjectFactory(this.MetaPopulation, typeof(C1)),
                });

            this.Setup(database, populate);
        }

        public MetaPopulation MetaPopulation { get; }

        public M M { get; }

        public virtual Config Config { get; } = new Config { SetupSecurity = false };

        public ISession Session { get; private set; }

        public ITimeService TimeService => ((DatabaseScope) this.Session.Database.Scope()).TimeService;

        public IDerivationService DerivationService => ((DatabaseScope) this.Session.Database.Scope()).DerivationService;

        public TimeSpan? TimeShift
        {
            get => this.TimeService.Shift;

            set => this.TimeService.Shift = value;
        }

        public Mock<IAccessControlLists> AclsMock
        {
            get
            {
                var aclMock = new Mock<IAccessControlList>();
                aclMock.Setup(acl => acl.CanRead(It.IsAny<IPropertyType>())).Returns(true);
                aclMock.Setup(acl => acl.CanRead(It.IsAny<IRoleClass>())).Returns(true);
                var aclsMock = new Mock<IAccessControlLists>();
                aclsMock.Setup(acls => acls[It.IsAny<IObject>()]).Returns(aclMock.Object);
                return aclsMock;
            }
        }

        public void Dispose()
        {
            this.Session.Rollback();
            this.Session = null;
        }

        protected void Setup(IDatabase database, bool populate)
        {
            database.Init();

            database.RegisterDerivations();

            this.Session = database.CreateSession();

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
