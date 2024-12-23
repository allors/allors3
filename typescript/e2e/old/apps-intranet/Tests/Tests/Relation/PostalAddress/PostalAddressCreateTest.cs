// <copyright file="PostalAddressCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.person.list;
using libs.workspace.angular.apps.src.lib.objects.person.overview;

namespace Tests.PostalAddressTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class PostalAddressCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly PersonListComponent people;

        private readonly PostalAddress editContactMechanism;

        public PostalAddressCreateTest(Fixture fixture)
            : base(fixture)
        {
            var person = new People(this.Transaction).Extent().FirstOrDefault();

            this.editContactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithDefaults()
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
            var country = new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE");

            var before = new PostalAddresses(this.Transaction).Extent().ToArray();

            var person = new People(this.Transaction).Extent().FirstOrDefault();

            this.people.Table.DefaultAction(person);
            var postalAddressEditComponent = new PersonOverviewComponent(this.people.Driver, this.M).ContactmechanismOverviewPanel.Click().CreatePostalAddress();

            postalAddressEditComponent
                .ContactPurposes.Toggle(new ContactMechanismPurposes(this.Transaction).GeneralCorrespondence)
                .Address1.Set("addressline 1")
                .Address2.Set("addressline 2")
                .Address3.Set("addressline 3")
                .Locality.Set("city")
                .PostalCode.Set("postalcode")
                .Country.Select(country)
                .Description.Set("description")
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PostalAddresses(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var contactMechanism = after.Except(before).First();
            var partyContactMechanism = contactMechanism.PartyContactMechanismsWhereContactMechanism.FirstOrDefault();

            Assert.Equal("addressline 1", contactMechanism.Address1);
            Assert.Equal("addressline 2", contactMechanism.Address2);
            Assert.Equal("addressline 3", contactMechanism.Address3);
            Assert.Equal("addressline 1", contactMechanism.Address1);
            Assert.Equal("city", contactMechanism.Locality);
            Assert.Equal("postalcode", contactMechanism.PostalCode);
            Assert.Equal(country, contactMechanism.Country);
            Assert.Equal("description", contactMechanism.Description);
        }
    }
}
