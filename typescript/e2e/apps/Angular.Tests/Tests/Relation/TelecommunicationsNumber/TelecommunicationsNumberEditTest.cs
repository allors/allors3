// <copyright file="TelecommunicationsNumberEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.person.list;
using libs.workspace.angular.apps.src.lib.objects.person.overview;
using libs.workspace.angular.apps.src.lib.objects.telecommunicationsnumber.edit;

namespace Tests.TelecommunicationsNumberTests
{
    using Allors.Database.Domain;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class TelecommunicationsNumberEditTest : Test, IClassFixture<Fixture>
    {
        private readonly PersonListComponent people;

        private readonly TelecommunicationsNumber editContactMechanism;

        public TelecommunicationsNumberEditTest(Fixture fixture)
            : base(fixture)
        {
            var person = new People(this.Session).Extent().FirstOrDefault();

            this.editContactMechanism = new TelecommunicationsNumberBuilder(this.Session)
                .WithCountryCode("0032")
                .WithAreaCode("498")
                .WithContactNumber("123 456")
                .Build();

            var partyContactMechanism = new PartyContactMechanismBuilder(this.Session).WithContactMechanism(this.editContactMechanism).Build();
            person.AddPartyContactMechanism(partyContactMechanism);

            this.Session.Derive();
            this.Session.Commit();

            this.Login();
            this.people = this.Sidenav.NavigateToPeople();
        }

        [Fact]
        public void Edit()
        {
            var person = new People(this.Session).Extent().FirstOrDefault();

            var before = new TelecommunicationsNumbers(this.Session).Extent().ToArray();

            this.people.Table.DefaultAction(person);
            var personOverview = new PersonOverviewComponent(this.people.Driver, this.M);

            var contactMechanismOverview = personOverview.ContactmechanismOverviewPanel.Click();
            var row = contactMechanismOverview.Table.FindRow(this.editContactMechanism);
            var cell = row.FindCell("contact");
            cell.Click();

            var editComponent = new TelecommunicationsNumberEditComponent(this.Driver, this.M);
            editComponent
                .CountryCode.Set("111")
                .AreaCode.Set("222")
                .ContactNumber.Set("333")
                .ContactMechanismType.Select(new ContactMechanismTypes(this.Session).MobilePhone)
                .Description.Set("description")
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Session.Rollback();

            var after = new TelecommunicationsNumbers(this.Session).Extent().ToArray();

            Assert.Equal(after.Length, before.Length);

            Assert.Equal("111", this.editContactMechanism.CountryCode);
            Assert.Equal("222", this.editContactMechanism.AreaCode);
            Assert.Equal("333", this.editContactMechanism.ContactNumber);
            Assert.Equal(new ContactMechanismTypes(this.Session).MobilePhone, this.editContactMechanism.ContactMechanismType);
            Assert.Equal("description", this.editContactMechanism.Description);
        }
    }
}
