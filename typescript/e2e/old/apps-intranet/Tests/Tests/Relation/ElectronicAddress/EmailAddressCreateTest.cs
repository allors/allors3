// <copyright file="EmailAddressCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.person.list;
using libs.workspace.angular.apps.src.lib.objects.person.overview;

namespace Tests.ElectronicAddressTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class EmailAddressCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly PersonListComponent personListPage;

        public EmailAddressCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.personListPage = this.Sidenav.NavigateToPeople();
        }

        [Fact]
        public void Create()
        {
            var before = new EmailAddresses(this.Transaction).Extent().ToArray();

            var person = new People(this.Transaction).Extent().FirstOrDefault();

            this.personListPage.Table.DefaultAction(person);
            var emailAddressCreate = new PersonOverviewComponent(this.personListPage.Driver, this.M).ContactmechanismOverviewPanel.Click().CreateEmailAddress();

            emailAddressCreate
                .ContactPurposes.Toggle(new ContactMechanismPurposes(this.Transaction).GeneralPhoneNumber)
                .ElectronicAddressString.Set("me@myself.com")
                .Description.Set("description")
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new EmailAddresses(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var contactMechanism = after.Except(before).First();

            Assert.Equal("me@myself.com", contactMechanism.ElectronicAddressString);
            Assert.Equal("description", contactMechanism.Description);
        }
    }
}
