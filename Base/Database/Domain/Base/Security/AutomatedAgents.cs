// <copyright file="People.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
   

    public partial class AutomatedAgents
    {
        public static readonly Guid GuestId = new Guid("1261CB56-67F2-4725-AF7D-604A117ABBEC");
        public static readonly Guid SystemId = new Guid("037C4B36-5950-4D32-BA95-85CCED5668DD");

        private UniquelyIdentifiableCache<AutomatedAgent> cache;

        public UniquelyIdentifiableCache<AutomatedAgent> Cache => this.cache ??= new UniquelyIdentifiableCache<AutomatedAgent>(this.Transaction);

        public AutomatedAgent Guest => this.Cache[GuestId];

        public AutomatedAgent System => this.Cache[SystemId];

        protected override void CorePrepare(Setup setup)
        {
            setup.AddDependency(this.ObjectType, this.M.UserGroup);
            setup.AddDependency(this.ObjectType, this.M.SecurityToken);
        }

        protected override void CoreSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            var guest = merge(GuestId, v => v.UserName = "Guest");
            merge(SystemId, v => v.UserName = "System");

            var userGroups = new UserGroups(this.Transaction);
            userGroups.Guests.AddMember(guest);
        }
    }
}
