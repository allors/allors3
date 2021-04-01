// <copyright file="DomainTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Allors.Database;
    using Allors.Database.Adapters.Memory;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Allors.Database.Security;
    using Allors.Protocol.Json.Api;
    using C1 = Allors.Database.Domain.C1;
    using User = Allors.Database.Domain.User;

    public class ApiTest : IDisposable
    {
        public ApiTest(Fixture fixture, bool populate = true)
        {
            var database = new Database(
                new DefaultDatabaseContext(fixture.Engine),
                new Configuration
                {
                    M = fixture.M,
                    ObjectFactory = new ObjectFactory(fixture.MetaPopulation, typeof(C1)),
                });

            this.M = database.Context().M;

            this.Setup(database, populate);
        }

        public M M { get; }

        public virtual Config Config { get; } = new Config { SetupSecurity = true };

        public ITransaction Transaction { get; private set; }

        public ITime Time => this.Transaction.Database.Context().Time;

        public IDerivationFactory DerivationFactory => this.Transaction.Database.Context().DerivationFactory;

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

            this.Transaction = database.CreateTransaction();

            if (populate)
            {
                new Setup(this.Transaction, this.Config).Apply();
                this.Transaction.Commit();
            }
        }

        protected User SetUser(string userName) => this.Transaction.Context().User = new Users(this.Transaction).FindBy(this.M.User.UserName, userName);

        protected Func<IAccessControlList, string> PrintAccessControls =>
            acl =>
            {
                var orderedAcls = acl.AccessControls.OrderBy(v => v).Select(v => v.Strategy.ObjectId.ToString()).ToArray();
                return orderedAcls.Any() ? string.Join(Encoding.Separator, orderedAcls) : null;
            };

        protected Func<IAccessControlList, string> PrintDeniedPermissions =>
            acl =>
            {
                var orderedDeniedPermissions = acl.DeniedPermissionIds.OrderBy(v => v).Select(v => v.ToString()).ToArray();
                return orderedDeniedPermissions.Any() ? string.Join(Encoding.Separator, orderedDeniedPermissions) : null;
            };

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
