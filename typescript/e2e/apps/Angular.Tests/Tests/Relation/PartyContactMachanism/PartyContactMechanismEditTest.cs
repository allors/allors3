// <copyright file="PartyContactMechanismEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;
using libs.workspace.angular.apps.src.lib.objects.person.list;

namespace Tests.PartyContactMachanismTests
{
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.person.overview;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class PartyContactMechanismEditTest : Test, IClassFixture<Fixture>
    {
        private readonly PersonListComponent people;

        private readonly PartyContactMechanism editPartyContactMechanism;

        public PartyContactMechanismEditTest(Fixture fixture)
            : base(fixture)
        {
            //var person = new People(this.Transaction).Extent().FirstOrDefault();

            //var postalAddress = new PostalAddressBuilder(this.Transaction)
            //    .WithAddress1("Haverwerf 15")
            //    .WithLocality("city")
            //    .WithPostalCode("1111")
            //    .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE"))
            //    .Build();

            //this.editPartyContactMechanism = new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(postalAddress).Build();
            //person.AddPartyContactMechanism(this.editPartyContactMechanism);

            //this.Transaction.Derive();
            //this.Transaction.Commit();

            this.Login();
            this.people = this.Sidenav.NavigateToPeople();
        }

        [Fact]
        public void Edit()
        {
            //var country = new Countries(this.Session).FindBy(M.Country.IsoCode, "NL");

            var before = new PartyContactMechanisms(this.Transaction).Extent().ToArray();

            var person = new People(this.Transaction).Extent().FirstOrDefault();
            var contactMechanismPurpose = new ContactMechanismPurposes(this.Transaction).Extent().FirstOrDefault();
            var contactMechanism = new ContactMechanisms(this.Transaction).Extent().FirstOrDefault(x => x.PeopleWhereCurrentOrganisationContactMechanism.Select(x => x.Id == person.Id).Any());

            var expected = new PartyContactMechanismBuilder(this.Transaction)
                .WithDefaults(contactMechanismPurpose, contactMechanism)
                .Build();


            this.Transaction.Derive();

            var expectedContactPurpose =  expected.ContactPurposes.ToList();
            var expectedContactMechanism = expected.ContactMechanism;
            var expectedFromDate = expected.FromDate;
            var expectedThroughDate = expected.ThroughDate;

            this.people.Table.DefaultAction(person);
            var personDetails = new PersonOverviewComponent(this.people.Driver, this.M);
            var PartyrateDetail = personDetails.PartycontactmechanismOverviewPanel.Click().CreatePartyContactMechanism();

            PartyrateDetail
                .FromDate.Set(expectedFromDate)
                .ThroughDate.Set(expectedThroughDate.Value)
                .ContactPurposes.Select(expectedContactPurpose.FirstOrDefault());

            Actions builder = new Actions(this.Driver);
            IAction keydown = builder.SendKeys(Keys.Escape).Build();
            keydown.Perform();

            PartyrateDetail
                .ContactMechanism.Select(expectedContactMechanism);

            this.Transaction.Rollback();
            PartyrateDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PartyContactMechanisms(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var partyContactMechanism = after.Except(before).First();

            Assert.Equal(expectedFromDate.Date, partyContactMechanism.FromDate.Date);
            Assert.Equal(expectedThroughDate.Value.Date, partyContactMechanism.ThroughDate.Value.Date);
            Assert.Equal(expectedContactPurpose.FirstOrDefault(), partyContactMechanism.ContactPurposes.FirstOrDefault());
            Assert.Equal(expectedContactMechanism, partyContactMechanism.ContactMechanism);
        }
    }
}
