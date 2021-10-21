// <copyright file="WebAddressEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;
using libs.workspace.angular.apps.src.lib.objects.person.list;
using libs.workspace.angular.apps.src.lib.objects.person.overview;
using libs.workspace.angular.apps.src.lib.objects.webaddress.edit;

namespace Tests.ElectronicAddressTests
{
    using Allors.Database.Domain;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class WebAddressEditTest : Test, IClassFixture<Fixture>
    {
        private readonly PersonListComponent personListPage;

        private readonly WebAddress editContactMechanism;

        public WebAddressEditTest(Fixture fixture)
            : base(fixture)
        {
            var person = new People(this.Transaction).Extent().FirstOrDefault();

            this.editContactMechanism = new WebAddressBuilder(this.Transaction)
                .WithElectronicAddressString("www.acme.com")
                .Build();

            var partyContactMechanism = new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(editContactMechanism).Build();
            person.AddPartyContactMechanism(partyContactMechanism);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Login();
            this.personListPage = this.Sidenav.NavigateToPeople();
        }

        [Fact]
        public void Edit()
        {
            var person = new People(this.Transaction).Extent().FirstOrDefault();

            var before = new WebAddresses(this.Transaction).Extent().ToArray();

            this.personListPage.Table.DefaultAction(person);
            var personOverview = new PersonOverviewComponent(this.personListPage.Driver, this.M);

            var contactMechanismOverview = personOverview.ContactmechanismOverviewPanel.Click();
            contactMechanismOverview.Table.DefaultAction(editContactMechanism);

            var webAddressEdit = new WebAddressEditComponent(this.Driver, this.M);
            webAddressEdit
                .ElectronicAddressString.Set("wwww.allors.com")
                .Description.Set("description")
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new WebAddresses(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length);

            Assert.Equal("wwww.allors.com", editContactMechanism.ElectronicAddressString);
            Assert.Equal("description", editContactMechanism.Description);
        }
    }
}
