// <copyright file="Singletons.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class SecurityTokens
    {
        public static readonly Guid InitialSecurityTokenId = new Guid("BE3404FF-1FF1-4C26-935F-777DC0AF983C");
        public static readonly Guid DefaultSecurityTokenId = new Guid("EF20E782-0BFB-4C59-B9EB-DC502C2256CA");
        public static readonly Guid AdministratorSecurityTokenId = new Guid("8C7FE74E-A769-49FC-BF69-549DBABD55D8");

        private UniquelyIdentifiableCache<SecurityToken> cache;

        public SecurityToken InitialSecurityToken => this.Cache[InitialSecurityTokenId];

        public SecurityToken DefaultSecurityToken => this.Cache[DefaultSecurityTokenId];

        public SecurityToken AdministratorSecurityToken => this.Cache[AdministratorSecurityTokenId];

        private UniquelyIdentifiableCache<SecurityToken> Cache => this.cache ??= new UniquelyIdentifiableCache<SecurityToken>(this.Transaction);

        protected override void CorePrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.Grant);

        protected override void CoreSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            var accessControls = new Grants(this.Transaction);

            merge(InitialSecurityTokenId, v =>
              {
                  if (setup.Config.SetupSecurity)
                  {
                      v.AddGrant(accessControls.Creators);
                      v.AddGrant(accessControls.GuestCreator);
                      v.AddGrant(accessControls.Administrator);
                  }
              });

            merge(DefaultSecurityTokenId, v =>
              {
                  if (setup.Config.SetupSecurity)
                  {
                      v.AddGrant(accessControls.Administrator);
                      v.AddGrant(accessControls.Guest);
                  }
              });

            merge(AdministratorSecurityTokenId, v =>
              {
                  if (setup.Config.SetupSecurity)
                  {
                      v.AddGrant(accessControls.Administrator);
                  }
              });
        }
    }
}
