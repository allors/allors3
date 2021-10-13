// <copyright file="PostalAddressEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;
using libs.workspace.angular.apps.src.lib.objects.person.list;
using libs.workspace.angular.apps.src.lib.objects.person.overview;
using libs.workspace.angular.apps.src.lib.objects.postaladdress.edit;

namespace Tests.PostalAddressTests
{
    using Allors.Database.Domain;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class PostalAddressEditTest : Test, IClassFixture<Fixture>
    {
        private readonly PersonListComponent people;

        private readonly PostalAddress editContactMechanism;

        public PostalAddressEditTest(Fixture fixture)
            : base(fixture)
        {
            var person = new People(this.Session).Extent().FirstOrDefault();

            this.editContactMechanism = new PostalAddressBuilder(this.Session)
                .WithAddress1("Haverwerf 15")
                .WithLocality("city")
                .WithPostalCode("1111")
                .WithCountry(new Countries(this.Session).FindBy(M.Country.IsoCode, "BE"))
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
            var country = new Countries(this.Session).FindBy(M.Country.IsoCode, "NL");

            var person = new People(this.Session).Extent().FirstOrDefault();

            var before = new PostalAddresses(this.Session).Extent().ToArray();

            this.people.Table.DefaultAction(person);
            var personOverview = new PersonOverviewComponent(this.people.Driver, this.M);

            var panelComponent = personOverview.ContactmechanismOverviewPanel.Click();
            var row = panelComponent.Table.FindRow(this.editContactMechanism);
            var cell = row.FindCell("contact");
            cell.Click();

            var postalAddressEditComponent = new PostalAddressEditComponent(this.Driver, this.M);
            postalAddressEditComponent.Address1.Set("addressline 1")
                .Address2.Set("addressline 2")
                .Address3.Set("addressline 3")
                .Locality.Set("city")
                .PostalCode.Set("postalcode")
                .Country.Select(country)
                .Description.Set("description")
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Session.Rollback();

            var after = new PostalAddresses(this.Session).Extent().ToArray();

            Assert.Equal(after.Length, before.Length);

            Assert.Equal("addressline 1", this.editContactMechanism.Address1);
            Assert.Equal("addressline 2", this.editContactMechanism.Address2);
            Assert.Equal("addressline 3", this.editContactMechanism.Address3);
            Assert.Equal("addressline 1", this.editContactMechanism.Address1);
            Assert.Equal("city", this.editContactMechanism.Locality);
            Assert.Equal("postalcode", this.editContactMechanism.PostalCode);
            Assert.Equal(country, this.editContactMechanism.Country);
            Assert.Equal("description", this.editContactMechanism.Description);
        }
    }
}
