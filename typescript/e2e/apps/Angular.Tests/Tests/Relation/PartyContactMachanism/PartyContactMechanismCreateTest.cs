// <copyright file="PartyContactMechanismCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;
using libs.workspace.angular.apps.src.lib.objects.person.list;

namespace Tests.PartyContactMachanismTests
{
    using Allors.Database.Domain;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class PartyContactMechanismCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly PersonListComponent people;

        private readonly PartyContactMechanism editPartyContactMechanism;

        public PartyContactMechanismCreateTest(Fixture fixture)
            : base(fixture)
        {
            var person = new People(this.Transaction).Extent().FirstOrDefault();

            var postalAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("city")
                .WithPostalCode("1111")
                .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE"))
                .Build();

            this.editPartyContactMechanism = new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(postalAddress).Build();
            person.AddPartyContactMechanism(this.editPartyContactMechanism);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Login();
            this.people = this.Sidenav.NavigateToPeople();
        }

        [Fact]
        public void Create()
        {
            // var country = new Countries(this.Session).FindBy(M.Country.IsoCode, "BE");

            // var before = new PostalAddresses(this.Session).Extent().ToArray();

            // var extent = new People(this.Session).Extent();
            // var person = extent.First(v => v.DisplayName().Equals("John Doe"));

            // var personOverview = this.people.Select(person);
            // var page = personOverview.NewPostalAddress();

            // .FromDate.Set(DateTimeFactory.CreateDate(2018, 12, 28);
            // .ThroughDate.Set(DateTimeFactory.CreateDate(DateTime.Now).AddYears(1);
            // .ContactPurposes.Toggle(new ContactMechanismPurposes(this.Session).BillingAddress.Name);
            // .ContactPurposes.Toggle(new ContactMechanismPurposes(this.Session).HeadQuarters.Name);
            // .Address1.Set("addressline 1";
            // .Address2.Set("addressline 2";
            // .Address3.Set("addressline 3";
            // .Locality.Set("city";
            // .PostalCode.Set("postalcode";
            // .Country.Set(country.Name;
            // .UseAsDefault.Set(true;
            // .NonSolicitationIndicator.Set(true;
            // .Description.Set("description";

            // .Save.Click();

            // this.Driver.WaitForAngular();
            // this.Session.Rollback();

            // var after = new PostalAddresses(this.Session).Extent().ToArray();

            // Assert.Equal(after.Length, before.Length + 1);

            // var contactMechanism = after.Except(before).First();
            // var partyContactMechanism = contactMechanism.PartyContactMechanismsWhereContactMechanism.FirstOrDefault();

            ////Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 28).Date, partyContactMechanism.FromDate.ToUniversalTime().Date);
            ////Assert.Equal(DateTimeFactory.CreateDate(DateTime.Now).AddYears(1).Date, partyContactMechanism.ThroughDate.Value.ToUniversalTime().Date);
            // Assert.Equal(2, partyContactMechanism.ContactPurposes.Count());
            // Assert.Contains(new ContactMechanismPurposes(this.Session).BillingAddress, partyContactMechanism.ContactPurposes);
            // Assert.Contains(new ContactMechanismPurposes(this.Session).HeadQuarters, partyContactMechanism.ContactPurposes);
            // Assert.Equal("addressline 1", contactMechanism.Address1);
            // Assert.Equal("addressline 2", contactMechanism.Address2);
            // Assert.Equal("addressline 3", contactMechanism.Address3);
            // Assert.Equal("addressline 1", contactMechanism.Address1);
            // Assert.Equal("city", contactMechanism.PostalBoundary.Locality);
            // Assert.Equal("postalcode", contactMechanism.PostalBoundary.PostalCode);
            // Assert.Equal(country, contactMechanism.PostalBoundary.Country);
            // Assert.True(partyContactMechanism.UseAsDefault);
            // Assert.True(partyContactMechanism.NonSolicitationIndicator);
            // Assert.Equal("description", contactMechanism.Description);
        }
    }
}
