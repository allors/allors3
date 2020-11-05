// <copyright file="OrganisationTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Domain
{
    using System;
    using Xunit;
    using ZXing.OneD;

    public class OrganisationTests : DomainTest, IClassFixture<Fixture>
    {
        public OrganisationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenOrganisation_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new OrganisationBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithName("Organisation");
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOrganisation_WhenActiveContactRelationship_ThenOrganisationCurrentOrganisationContactRelationshipsContainsOrganisation()
        {
            var contact = new PersonBuilder(this.Session).WithLastName("organisationContact").Build();
            var organisation = new OrganisationBuilder(this.Session).WithName("organisation").Build();

            new CustomerRelationshipBuilder(this.Session)
                .WithCustomer(organisation)
                .WithFromDate(DateTimeFactory.CreateDate(2010, 01, 01))
                .Build();

            new OrganisationContactRelationshipBuilder(this.Session)
                .WithContact(contact)
                .WithOrganisation(organisation)
                .WithFromDate(this.Session.Now().Date)
                .Build();

            this.Session.Derive();

            Assert.Equal(contact.CurrentOrganisationContactRelationships[0].Organisation, organisation);
            Assert.Equal(0, contact.InactiveOrganisationContactRelationships.Count);
        }

        [Fact]
        public void GivenOrganisation_WhenInActiveContactRelationship_ThenOrganisationnactiveOrganisationContactRelationshipsContainsOrganisation()
        {
            var contact = new PersonBuilder(this.Session).WithLastName("organisationContact").Build();
            var organisation = new OrganisationBuilder(this.Session).WithName("organisation").Build();

            new CustomerRelationshipBuilder(this.Session)
                .WithCustomer(organisation)
                .WithFromDate(DateTimeFactory.CreateDate(2010, 01, 01))
                .Build();

            new OrganisationContactRelationshipBuilder(this.Session)
                .WithContact(contact)
                .WithOrganisation(organisation)
                .WithFromDate(this.Session.Now().Date.AddDays(-1))
                .WithThroughDate(this.Session.Now().Date.AddDays(-1))
                .Build();

            this.Session.Derive();

            Assert.Equal(contact.InactiveOrganisationContactRelationships[0].Organisation, organisation);
            Assert.Equal(0, contact.CurrentOrganisationContactRelationships.Count);
        }
    }

    [Trait("Category", "Security")]
    public class OrganisationDeniedPermissionTests : DomainTest, IClassFixture<Fixture>
    {
        public OrganisationDeniedPermissionTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.Organisation.ObjectType, this.M.Organisation.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;


        [Fact]
        public void OnChangeOrganisationDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationIsInternalOrganisationDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).WithIsInternalOrganisation(true).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithExternalAccountingTransactionFromPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var externalAccountingTransaction = new SalesAccountingTransactionBuilder(this.Session)
                .WithFromParty(organisation).Build();

            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithExternalAccountingTransactionToPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var externalAccountingTransaction = new SalesAccountingTransactionBuilder(this.Session)
                .WithToParty(organisation).Build();

            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithShipmentFromPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipment = new TransferBuilder(this.Session).WithShipFromParty(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithShipmentToPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipment = new TransferBuilder(this.Session).WithShipToParty(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPaymentReceiverDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var payment = new ReceiptBuilder(this.Session).WithReceiver(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPaymentSenderDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var payment = new ReceiptBuilder(this.Session).WithSender(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithEmploymentDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var employment = new EmploymentBuilder(this.Session).WithEmployer(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithEngagementBillToPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var engagement = new EngagementBuilder(this.Session).WithBillToParty(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithEngagementPlacingPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var engagement = new EngagementBuilder(this.Session).WithPlacingParty(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPartManufacturedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var part = new NonUnifiedPartBuilder(this.Session).WithManufacturedBy(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPartSuppliedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var part = new NonUnifiedPartBuilder(this.Session).Build();

            var supplierOffering = new SupplierOfferingBuilder(this.Session)
                .WithFromDate(DateTime.Now.AddDays(-1))
                .WithThroughDate(DateTime.Now.AddDays(5))
                .WithSupplier(organisation)
                .WithPart(part)
                .Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithOrganisationGlAccountDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var organisationGlAccount = new OrganisationGlAccountBuilder(this.Session).WithInternalOrganisation(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithOrganisationRollUpDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var organisationRollUp = new OrganisationRollUpBuilder(this.Session).WithParent(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }
    }
}
