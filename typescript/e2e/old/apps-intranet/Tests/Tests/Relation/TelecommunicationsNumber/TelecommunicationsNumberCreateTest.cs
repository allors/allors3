// <copyright file="TelecommunicationsNumberCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.person.list;
using libs.workspace.angular.apps.src.lib.objects.person.overview;
using libs.workspace.angular.apps.src.lib.objects.telecommunicationsnumber.create;

namespace Tests.TelecommunicationsNumberTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class TelecommunicationsNumberCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly PersonListComponent people;

        private readonly TelecommunicationsNumber editContactMechanism;

        public TelecommunicationsNumberCreateTest(Fixture fixture)
            : base(fixture)
        {
            var person = new People(this.Transaction).Extent().FirstOrDefault();

            this.editContactMechanism = new TelecommunicationsNumberBuilder(this.Transaction)
                .WithCountryCode("0032")
                .WithAreaCode("498")
                .WithContactNumber("123 456")
                .Build();

            var partyContactMechanism = new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(this.editContactMechanism).Build();
            person.AddPartyContactMechanism(partyContactMechanism);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Login();
            this.people = this.Sidenav.NavigateToPeople();
        }

        [Fact]
        public void Create()
        {
            var before = new TelecommunicationsNumbers(this.Transaction).Extent().ToArray();

            var person = new People(this.Transaction).Extent().FirstOrDefault();

            this.people.Table.DefaultAction(person);
            new PersonOverviewComponent(this.people.Driver, this.M).ContactmechanismOverviewPanel.Click().CreateTelecommunicationsNumber();

            var createComponent = new TelecommunicationsNumberCreateComponent(this.Driver, this.M);
            createComponent
                .ContactPurposes.Toggle(new ContactMechanismPurposes(this.Transaction).GeneralPhoneNumber)
                .CountryCode.Set("111")
                .AreaCode.Set("222")
                .ContactNumber.Set("333")
                .ContactMechanismType.Select(new ContactMechanismTypes(this.Transaction).MobilePhone)
                .Description.Set("description")
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new TelecommunicationsNumbers(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var contactMechanism = after.Except(before).First();

            Assert.Equal("111", contactMechanism.CountryCode);
            Assert.Equal("222", contactMechanism.AreaCode);
            Assert.Equal("333", contactMechanism.ContactNumber);
            Assert.Equal(new ContactMechanismTypes(this.Transaction).MobilePhone, contactMechanism.ContactMechanismType);
            Assert.Equal("description", contactMechanism.Description);
        }
    }
}
