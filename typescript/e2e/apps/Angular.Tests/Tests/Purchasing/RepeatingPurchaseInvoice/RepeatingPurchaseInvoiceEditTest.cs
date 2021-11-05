// <copyright file="PartyContactMechanismEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace Tests.RepeatingPurchaseInvoice
{
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.organisation.list;
    using libs.workspace.angular.apps.src.lib.objects.organisation.overview;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class RepeatingPurchaseInvoiceEditTest : Test, IClassFixture<Fixture>
    {
        private readonly OrganisationListComponent Organisations;

        public RepeatingPurchaseInvoiceEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.Organisations = this.Sidenav.NavigateToOrganisations();
        }

        [Fact]
        public void Edit()
        {
            var before = new RepeatingPurchaseInvoices(this.Transaction).Extent().ToArray();

            var organisation = new Organisations(this.Transaction).Extent().FirstOrDefault(x => x.ExistSupplierOfferingsWhereSupplier);

            var expected = new RepeatingPurchaseInvoiceBuilder(this.Transaction)
                .WithDefaults(organisation)
                .Build();

            this.Transaction.Derive();

            var expectedInternalOrganisation = expected.InternalOrganisation;
            var expectedFrequency = expected.Frequency;
            var expectedNextExecutionDate = expected.NextExecutionDate;
            var expectedDayOfWeek = expected.DayOfWeek;

            this.Organisations.Table.DefaultAction(organisation);
            var organisationOverviewComponent = new OrganisationOverviewComponent(this.Organisations.Driver, this.M);
            var repeatingPurchaseInvoiceDetail = organisationOverviewComponent.RepeatingpurchaseinvoiceOverviewPanel.Click().CreateRepeatingPurchaseInvoice();

            repeatingPurchaseInvoiceDetail
                .InternalOrganisation.Select(expectedInternalOrganisation)
                .Frequency.Select(expectedFrequency)
                .NextExecutionDate.Set(expectedNextExecutionDate)
                .DayOfWeek.Select(expectedDayOfWeek);

            this.Transaction.Rollback();
            repeatingPurchaseInvoiceDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new RepeatingPurchaseInvoices(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var repeatingPurchaseInvoice = after.Except(before).First();

            Assert.Equal(expectedInternalOrganisation, repeatingPurchaseInvoice.InternalOrganisation);
            Assert.Equal(expectedFrequency, repeatingPurchaseInvoice.Frequency);
            Assert.Equal(expectedNextExecutionDate.Date, repeatingPurchaseInvoice.NextExecutionDate.Date);
            Assert.Equal(expectedDayOfWeek, repeatingPurchaseInvoice.DayOfWeek);
        }
    }
}
