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
    using libs.workspace.angular.apps.src.lib.objects.positiontype.list;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "HumanResource")]
    public class PositionTypeEditTest : Test, IClassFixture<Fixture>
    {
        private readonly PositionTypesOverviewComponent positionTypes;

        public PositionTypeEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.positionTypes = this.Sidenav.NavigateToPositionTypes();
        }

        [Fact]
        public void Edit()
        {
            var before = new PositionTypes(this.Transaction).Extent().ToArray();

            var expected = new PositionTypeBuilder(this.Transaction)
                .WithTitle("test title")
                .WithDescription("test description")
                .Build();

            this.Transaction.Derive();

            var expectedTitle = expected.Title;
            var expectedDescription = expected.Description;

            var positionTypeDetails = new PositionTypesOverviewComponent(this.positionTypes.Driver, this.M);
            var positionTypeOverview =  positionTypeDetails.CreatePositionType();

            positionTypeOverview
                .Title.Set(expectedTitle)
                .Description.Set(expectedDescription);

            this.Transaction.Rollback();
            positionTypeOverview.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PositionTypes(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var positionType = after.Except(before).First();

            Assert.Equal(expectedTitle, positionType.Title);
            Assert.Equal(expectedDescription, positionType.Description);
        }
    }
}
