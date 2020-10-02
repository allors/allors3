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
    using Allors;
    using Allors.Database.Adapters.Memory;
    using Allors.Domain;
    using Allors.Meta;
    using Allors.Protocol.Remote;
    using Allors.Services;

    public class ApiTest : IDisposable
    {
        public ApiTest(Fixture fixture, bool populate = true)
        {
            var database = new Database(
                new DefaultDatabaseScope(),
                new Configuration
                {
                    ObjectFactory = new ObjectFactory(fixture.MetaPopulation, typeof(C1)),
                });

            this.M = database.Scope().M;

            this.Setup(database, populate);
        }

        public M M { get; }

        public virtual Config Config { get; } = new Config { SetupSecurity = true };

        public ISession Session { get; private set; }

        public ITimeService TimeService => this.Session.Database.Scope().TimeService;

        public IDerivationService DerivationService => this.Session.Database.Scope().DerivationService;

        public TimeSpan? TimeShift
        {
            get => this.TimeService.Shift;

            set => this.TimeService.Shift = value;
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

        protected User SetUser(string userName) => this.Session.Scope().User = new Users(this.Session).FindBy(this.M.User.UserName, userName);

        protected Func<IAccessControlList, string> PrintAccessControls =>
            acl =>
            {
                var orderedAcls = acl.AccessControls.OrderBy(v => v).Select(v => v.Id.ToString()).ToArray();
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
