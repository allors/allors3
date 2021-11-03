// <copyright file="EmailAddressEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;
using libs.workspace.angular.apps.src.lib.objects.emailaddress.edit;
using libs.workspace.angular.apps.src.lib.objects.salesinvoice.list;
using libs.workspace.angular.apps.src.lib.objects.person.overview;

namespace Tests.SalesInvoiceTests
{
    using Allors.Database.Domain;
    using Components;

    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class RepeatingSalesInvoiceCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly SalesInvoiceListComponent salesInvoiceListPage;

        public RepeatingSalesInvoiceCreateTest(Fixture fixture) : base(fixture)
        {
            this.Login();
            this.salesInvoiceListPage = this.Sidenav.NavigateToSalesInvoices();
        }

        [Fact]
        public void Create()
        {
            var person = new People(this.Transaction).Extent().FirstOrDefault();

            var electronicAddress = new EmailAddressBuilder(this.Transaction)
                .WithElectronicAddressString("info@acme.com")
                .Build();

            var partyContactMechanism = new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(electronicAddress).Build();
            person.AddPartyContactMechanism(partyContactMechanism);

            this.Transaction.Derive();
            this.Transaction.Commit();

            var before = new EmailAddresses(this.Transaction).Extent().ToArray();

            this.salesInvoiceListPage.Table.DefaultAction(person);
            var personOverviewComponent = new PersonOverviewComponent(this.salesInvoiceListPage.Driver, this.M);

            var contactMechanismOverviewPanel = personOverviewComponent.ContactmechanismOverviewPanel.Click();
            contactMechanismOverviewPanel.Table.DefaultAction(electronicAddress);

            var emailAddressEditComponent = new EmailAddressEditComponent(this.Driver, this.M);
            emailAddressEditComponent
                .ElectronicAddressString.Set("me@myself.com")
                .Description.Set("description")
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new EmailAddresses(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length);

            Assert.Equal("me@myself.com", electronicAddress.ElectronicAddressString);
            Assert.Equal("description", electronicAddress.Description);
        }
    }
}
