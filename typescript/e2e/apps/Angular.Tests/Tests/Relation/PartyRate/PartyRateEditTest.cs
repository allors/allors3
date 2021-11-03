// <copyright file="PartyContactMechanismEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace Tests.PartyContactMachanismTests
{
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.organisation.list;
    using libs.workspace.angular.apps.src.lib.objects.organisation.overview;
    using libs.workspace.angular.apps.src.lib.objects.partyrate.edit;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class PartyRateEditTest : Test, IClassFixture<Fixture>
    {
        private readonly OrganisationListComponent organisation;

        //private readonly PartyContactMechanism editPartyContactMechanism;

        public PartyRateEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.organisation = this.Sidenav.NavigateToOrganisations();
        }

        [Fact]
        public void Edit()
        {
            var before = new PartyRates(this.Transaction).Extent().ToArray();

            var expected = new PartyRateBuilder(this.Transaction)
                .WithDefaults()
                .Build();
            var organisation = new Organisations(this.Transaction).Extent().FirstOrDefault();


            this.Transaction.Derive();

            var expectedRateType = expected.RateType;
            var expectedRate = expected.Rate;
            var expectedFromDate = expected.FromDate;
            var expectedThroughDate = expected.ThroughDate;

            this.organisation.Table.DefaultAction(organisation);
            var organisationDetails = new OrganisationOverviewComponent(this.organisation.Driver, this.M);
            var PartyrateDetail = organisationDetails.PartyrateOverviewPanel.Click().CreatePartyRate();

            PartyrateDetail
                .RateType.Select(expectedRateType)
                .Rate.Set(expectedRate.ToString())
                .FromDate.Set(expectedFromDate)
                .ThroughDate.Set(expectedThroughDate.Value);

            this.Transaction.Rollback();
            PartyrateDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PartyRates(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var partyRate = after.Except(before).First();

            Assert.Equal(expectedFromDate.Date, partyRate.FromDate.Date);
            Assert.Equal(expectedThroughDate.Value.Date, partyRate.ThroughDate.Value.Date);
            Assert.Equal(expectedRate, partyRate.Rate);
            Assert.Equal(expectedRateType, partyRate.RateType);
        }
    }
}
