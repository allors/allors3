// <copyright file="Setup.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;
    using Meta;
    using Services;

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

            // Security
            if (this.Config.SetupSecurity)
            {
                this.transaction.Database.Services.Get<IPermissions>().Sync(this.transaction);

                var denied = new DeniedBuilder(this.transaction)
                    .WithDatabaseProperty("DatabaseProp")
                    .WithDefaultWorkspaceProperty("DefaultWorkspaceProp")
                    .WithWorkspaceXProperty("WorkspaceXProp")
                    .Build();

                var m = denied.M;

                var databaseWrite = new Permissions(this.transaction).Extent().First(v => v.Operation == Operations.Write && v.OperandType.Equals(m.Denied.DatabaseProperty));
                var defaultWorkspaceWrite = new Permissions(this.transaction).Extent().First(v => v.Operation == Operations.Write && v.OperandType.Equals(m.Denied.DefaultWorkspaceProperty));
                var workspaceXWrite = new Permissions(this.transaction).Extent().First(v => v.Operation == Operations.Write && v.OperandType.Equals(m.Denied.WorkspaceXProperty));

                var revocation = new RevocationBuilder(this.transaction)
                    .WithDeniedPermission(databaseWrite)
                    .WithDeniedPermission(defaultWorkspaceWrite)
                    .WithDeniedPermission(workspaceXWrite)
                    .Build();

                denied.AddRevocation(revocation);
            }

            // Trimming
            if (this.Config.SetupSecurity)
            {
                // Objects
                var fromTrimmed1 = new TrimFromBuilder(this.transaction).WithName("Trimmed1").Build();
                var fromTrimmed2 = new TrimFromBuilder(this.transaction).WithName("Trimmed2").Build();
                var fromUntrimmed1 = new TrimFromBuilder(this.transaction).WithName("Untrimmed1").Build();
                var fromUntrimmed2 = new TrimFromBuilder(this.transaction).WithName("Untrimmed2").Build();

                var toTrimmed = new TrimToBuilder(this.transaction).WithName("Trimmed1").Build();
                var toUntrimmed = new TrimToBuilder(this.transaction).WithName("Untrimmed1").Build();

                var m = this.transaction.Database.Services.Get<MetaPopulation>();

                // Denied Permissions
                var fromTrimPermission = new Permissions(this.transaction).Extent().First(v => v.Operation == Operations.Read && v.OperandType.Equals(m.TrimFrom.Name));
                var fromRevocation = new RevocationBuilder(this.transaction)
                    .WithDeniedPermission(fromTrimPermission)
                    .Build();
                fromTrimmed1.AddRevocation(fromRevocation);
                fromTrimmed2.AddRevocation(fromRevocation);

                var toTrimPermission = new Permissions(this.transaction).Extent().First(v => v.Operation == Operations.Read && v.OperandType.Equals(m.TrimTo.Name));
                var toRevocation = new RevocationBuilder(this.transaction)
                    .WithDeniedPermission(toTrimPermission)
                    .Build();
                toTrimmed.AddRevocation(toRevocation);

                // Relations
                fromTrimmed1.Many2One = toTrimmed;
                fromTrimmed2.Many2One = toUntrimmed;
                fromUntrimmed1.Many2One = toTrimmed;
                fromUntrimmed2.Many2One = toUntrimmed;

                fromTrimmed1.AddMany2Many(toTrimmed);
                fromTrimmed2.AddMany2Many(toUntrimmed);
                fromUntrimmed1.AddMany2Many(toTrimmed);
                fromUntrimmed2.AddMany2Many(toUntrimmed);
            }
        }
    }
}
