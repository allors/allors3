// <copyright file="Setup.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Setup
    {
        private void CustomOnPrePrepare()
        {
        }

        private void CustomOnPostPrepare()
        {
        }

        private void CustomOnPreSetup()
        {
        }

        private void CustomOnPostSetup(Config config)
        {
            var jane = new PersonBuilder(this.transaction).WithFirstName("Jane").WithLastName("Doe").WithUserName("jane@example.com").Build();
            var john = new PersonBuilder(this.transaction).WithFirstName("John").WithLastName("Doe").WithUserName("john@example.com").Build();

            jane.SetPassword("jane");
            john.SetPassword("john");

            new UserGroups(this.transaction).Administrators.AddMember(jane);
            new UserGroups(this.transaction).Creators.AddMember(jane);
            new UserGroups(this.transaction).Creators.AddMember(john);
        }
    }
}
