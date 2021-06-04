// <copyright file="Setup.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Domain;

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

        private void CustomOnPostSetup()
        {

            var jane = new PersonBuilder(this.transaction).WithFirstName("Jane").WithLastName("Doe").WithUserName("jane@example.com").Build();
            var john = new PersonBuilder(this.transaction).WithFirstName("John").WithLastName("Doe").WithUserName("john@example.com").Build();
            var jenny = new PersonBuilder(this.transaction).WithFirstName("Jenny").WithLastName("Doe").WithUserName("jenny@example.com").Build();

            var guest = new PersonBuilder(this.transaction).WithFirstName("Gu").WithLastName("Est").WithUserName("guest@example.com").Build();

            jane.SetPassword("jane");
            john.SetPassword("john");
            jenny.SetPassword("jenny");

            new UserGroups(this.transaction).Administrators.AddMember(jane);
            new UserGroups(this.transaction).Creators.AddMember(jane);
            new UserGroups(this.transaction).Creators.AddMember(john);
            new UserGroups(this.transaction).Creators.AddMember(jenny);

            var acme = new OrganisationBuilder(this.transaction)
                .WithName("Acme")
                .WithOwner(jane)
                .WithEmployee(john)
                .WithEmployee(jenny)
                .Build();

            for (var i = 0; i < 100; i++)
            {
                new OrganisationBuilder(this.transaction)
                    .WithName($"Organisation-{i}")
                    .WithOwner(john)
                    .WithEmployee(jenny)
                    .WithEmployee(jane)
                    .Build();
            }

            // Create cycles between Organisation and Person
            var cycleOrganisation1 = new OrganisationBuilder(this.transaction).WithName("Organisatin Cycle One").Build();
            var cycleOrganisation2 = new OrganisationBuilder(this.transaction).WithName("Organisatin Cycle Two").Build();

            var cyclePerson1 = new PersonBuilder(this.transaction).WithFirstName("Person Cycle").WithLastName("One").WithUserName("cycle1@one.org").Build();
            var cyclePerson2 = new PersonBuilder(this.transaction).WithFirstName("Person Cycle").WithLastName("Two").WithUserName("cycle2@one.org").Build();

            // One
            cycleOrganisation1.CycleOne = cyclePerson1;
            cyclePerson1.CycleOne = cycleOrganisation1;

            cycleOrganisation2.CycleOne = cyclePerson2;
            cyclePerson2.CycleOne = cycleOrganisation2;

            // Many
            cycleOrganisation1.AddCycleMany(cyclePerson1);
            cycleOrganisation1.AddCycleMany(cyclePerson2);

            cycleOrganisation1.AddCycleMany(cyclePerson1);
            cycleOrganisation1.AddCycleMany(cyclePerson2);

            cyclePerson1.AddCycleMany(cycleOrganisation1);
            cyclePerson1.AddCycleMany(cycleOrganisation2);

            cyclePerson2.AddCycleMany(cycleOrganisation1);
            cyclePerson2.AddCycleMany(cycleOrganisation2);
        }
    }
}
