// <copyright file="Population.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.IO;
    using Allors.Database;
    using Allors.Database.Domain;

    public class Population
    {
        private readonly ITransaction session;

        private readonly DirectoryInfo dataPath;

        public Population(ITransaction session, DirectoryInfo dataPath)
        {
            this.session = session;
            this.dataPath = dataPath;
        }

        public void Execute()
        {
            var person = new PersonBuilder(this.session)
                .WithUserName("administrator")
                .Build();

            new UserGroups(this.session).Creators.AddMember(person);
            new UserGroups(this.session).Administrators.AddMember(person);

            this.session.Derive();
        }
    }
}
