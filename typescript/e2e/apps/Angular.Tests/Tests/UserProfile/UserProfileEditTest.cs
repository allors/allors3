// <copyright file="CustomerShipmentCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Tests.UserProfileTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.catalogue.list;
    using libs.workspace.angular.apps.src.lib.objects.userprofile.edit;
    using src.app.main;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Product")]
    public class UserProfileEditTest : Test, IClassFixture<Fixture>
    {
        public Person Person { get; }

        public UserProfileEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Person = this.Login();
        }

        [Fact]
        public void Edit()
        {
            var before = new UserProfiles(this.Transaction).Extent().ToArray();
            var userProfileId = this.Person.UserProfile.Id;

            var internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var locale = internalOrganisation.Locale;

            var expected = new UserProfileBuilder(this.Transaction)
                .WithDefaultInternalOrganization(internalOrganisation)
                .WithDefaulLocale(locale)
                .Build();

            this.Transaction.Derive();

            var expectedInternalOrganisation = expected.DefaultInternalOrganization;
            var expectedLocale = expected.DefaulLocale;

            new MainComponent(this.Driver, this.M).Sidenav.UserProfile.Click();
            var userProfileComponent = new UserProfileEditComponent(this.Driver, this.M);

            userProfileComponent
                .DefaultInternalOrganization.Select(expected.DefaultInternalOrganization)
                .DefaulLocale.Select(expected.DefaulLocale);

            this.Transaction.Rollback();
            userProfileComponent.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new UserProfiles(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length);

            var actual = after.First(v => v.Id == userProfileId);

            Assert.Equal(expectedInternalOrganisation, actual.DefaultInternalOrganization);
            Assert.Equal(expectedLocale, actual.DefaulLocale);
        }
    }
}
