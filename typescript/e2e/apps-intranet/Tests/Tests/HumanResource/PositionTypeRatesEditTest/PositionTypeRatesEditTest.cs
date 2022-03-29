// <copyright file="PartyContactMechanismEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;
using libs.workspace.angular.apps.src.lib.objects.person.list;

namespace Tests.PositionTypeRatesTests
{
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.person.overview;
    using libs.workspace.angular.apps.src.lib.objects.positiontype.list;
    using libs.workspace.angular.apps.src.lib.objects.positiontyperate.list;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "HumanResource")]
    public class PositionTypeRatesEditTest : Test, IClassFixture<Fixture>
    {
        private readonly PositionTypeRatesOverviewComponent positionTypeRates;

        public PositionTypeRatesEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.positionTypeRates = this.Sidenav.NavigateToPositionTypeRates();
        }

        [Fact]
        public void Edit()
        {
            var before = new PositionTypeRates(this.Transaction).Extent().ToArray();

            var expected = new PositionTypeRateBuilder(this.Transaction)
                .WithDefaults()
                .Build();


            this.Transaction.Derive();

            var expectedRateType = expected.RateType;
            var expectedRate = expected.Rate;
            var expectedFromDate = expected.FromDate;
            var expectedFrequency = expected.Frequency;

            //this.positionTypes.Table.DefaultAction(person);
            var positionTypeRateDetails = new PositionTypeRatesOverviewComponent(this.positionTypeRates.Driver, this.M);
            var positionTypeRateOverview =  positionTypeRateDetails.CreatePositionTypeRate();

            positionTypeRateOverview
                .RateType.Select(expectedRateType)
                .FromDate.Set(expectedFromDate)
                .Rate.Set(expectedRate.ToString())
                .Frequency.Select(expectedFrequency);

            this.Transaction.Rollback();
            positionTypeRateOverview.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PositionTypeRates(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var positionTypeRate = after.Except(before).First();

            Assert.Equal(expectedRateType, positionTypeRate.RateType);
            Assert.Equal(expectedRate, positionTypeRate.Rate);
            Assert.Equal(expectedFromDate.Date, positionTypeRate.FromDate.Date);
            Assert.Equal(expectedFrequency, positionTypeRate.Frequency);
        }
    }
}
