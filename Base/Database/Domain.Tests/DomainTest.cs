// <copyright file="DomainTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors
{
    using System;
    using System.Linq;
    using Allors.Database.Adapters.Memory;
    using Allors.Domain;
    using Allors.Meta;
    using Allors.Services;
    using Bogus;
    using Domain.Derivations;
    using Microsoft.Extensions.DependencyInjection;
    using Person = Domain.Person;

    public class DomainTest : IDisposable
    {
        public DomainTest(bool populate = true) => this.Setup(populate);

        public virtual Config Config { get; } = new Config { SetupSecurity = false };

        public ISession Session { get; private set; }

        public ITimeService TimeService => this.Session.ServiceProvider.GetRequiredService<ITimeService>();

        public TimeSpan? TimeShift
        {
            get => this.TimeService.Shift;

            set => this.TimeService.Shift = value;
        }

        protected Organisation InternalOrganisation => this.Session.Extent<Organisation>().First(v => v.IsInternalOrganisation);

        protected Person Administrator => this.GetPersonByUserName("administrator");

        protected Person OrderProcessor => this.GetPersonByUserName("orderProcessor");

        protected Person Purchaser => this.GetPersonByUserName("purchaser");

        protected ObjectFactory ObjectFactory => new ObjectFactory(MetaPopulation.Instance, typeof(User));

        public void Dispose()
        {
            this.Session.Rollback();
            this.Session = null;
        }

        protected void Setup(bool populate)
        {
#if ALLORS_DERIVATION_PERSISTENT
            var derivationPersistent = true;
#else
            var environmentVariable = Environment.GetEnvironmentVariable("ALLORS_DERIVATION");
            var derivationPersistent = environmentVariable?.ToLowerInvariant().Equals("persistent") == true;
#endif

            var services = new ServiceCollection();
            if (derivationPersistent)
            {
                services.AddAllors((session) => new Allors.Domain.Derivations.Persistent.Derivation(session, new DerivationConfig { MaxCycles = 10, MaxIterations = 10, MaxPreparations = 10 }));
            }
            else
            {
                services.AddAllors((session) => new Allors.Domain.Derivations.Default.Derivation(session, new DerivationConfig { MaxCycles = 10, MaxIterations = 10, MaxPreparations = 10 }));
            }

            services.AddSingleton<Faker>();
            var serviceProvider = services.BuildServiceProvider();

            var configuration = new Configuration
            {
                ObjectFactory = this.ObjectFactory,
            };

            var database = new Database.Adapters.Memory.Database(serviceProvider, configuration);
            this.Setup(database, populate);
        }

        protected void Setup(IDatabase database, bool populate)
        {
            database.Init();

            var derivations = new IDomainDerivation[]
            {
                new AuditableDerivation(),
                new ShipmentDerivation(),
                new SupplierOfferingDerivation(),
                new ServiceDerivation(),
                new SerialisedItemCharacteristicDerivation(),
                new SerialisedInventoryItemDerivation(),
                new ProductCategoryDerivation(),
                new PriceComponentDerivation(),
                new PartCategoryDerivation(),
                new OrderValueDerivation(),
                new OrderQuantityBreakDerivation(),
                new NonUnifiedGoodDerivation(),
                new StatementOfWorkDerivation(),
                new SurchargeComponentDerivation(),
                new SerialisedItemDerivation(),
                new QuoteItemDerivation(),
                new PurchaseOrderApprovalLevel1Derivation(),
                new ProposalDerivation(),
                new ProductQuoteApprovalDerivation(),
                new ProductQuoteDerivation(),
                new OrderAdjustmentDerivation(),
                new EngagementDerivation(),
                new SalesOrderTransferDerivation(),
                new QuoteDerivation(),
                new PurchaseOrderApprovalLevel2Derivation(),
                new PurchaseReturnDerivation(),
                new PurchaseShipmentDerivation(),
                new ShipmentPackageDerivation(),
                new ShipmentValueDerivation(),
                new PickListItemDerivation(),
                new PickListDerivation(),
                new PackagingContentDerivation(),
                new DropShipmentsDerivation(),
                new TransferDerivation(),
                new shipmentReceiptDerivation(),
                new CustomerReturnDerivation(),
                new SalesOrderItemInventoryAssignmentDerivation(),
                new UnifiedGoodDerivation(),
                new NonSerialisedInventoryItemDerivation(),
                new NonUnifiedPartDerivation(),
                new PartDerivation(),
                new InventoryItemTransactionDerivation(),
                new InventoryItemDerivation(),
                new CatalogueDerivation(),
                new PriceComponentDerivation(),
                new SingletonDerivation(),
                new SettingsDerivation(),
                new PhoneCommunicationDerivation(),
                new ProfessionalServicesRelationshipDerivation(),
                new OrganisationContactRelationshipDerivation(),
                new InternalOrganisationDerivation(),
                new FaceToFaceCommunicationDerivation(),
                new EmploymentDerivation(),
                new CommunicationTaskDerivation(),
                new CommunicationEventDerivation(),
                new AutomatedAgentDerivation(),
                new AgreementTermDerivation(),
                new EmailCommunicationDerivation(),
                new OrganisationDerivation(),
                new PersonDerivation(),
                new MediaDerivation(),
                new PartyDerivation(),
                new EmailTemplateDerivation(),
                new WebSiteCommunicationsDerivation(),
                new CustomerRelationshipDerivation(),
                new FaxCommunicationDerivation(),
                new LetterCorrespondenceDerivation(),
                new OrganisationRollUpDerivation(),
                new PartyContactMechanismDerivation(),
                new subcontractorRelationshipDerivation(),
                new PassportDerivation(),
                new RequestItemDerivation(),
                new RequestForQuoteDerivation(),
                new RequestForProposalDerivation(),
                new RequestForInformationDerivation(),
                new RequestDerivation(),
                new PurchaseInvoiceDerivation(),
                new PurchaseOrderItemDerivation(),
                new PaymentDerivation(),
                new PurchaseInvoiceApprovalDerivation(),
                new SalesInvoiceItemDerivation(),
                new RepeatingPurchaseInvoiceDerivation(),
                new RepeatingSalesInvoiceDerivation(),
                new SalesInvoiceDerivation(),
                new PartyFinancialRelationshipDerivation(),
                new PaymentApplicationDerivation(),
                new SupplierRelationshipDerivation(),
                new PurchaseOrderDerivation(), //Has Dependency on SupplierRelationship

                new ShipmentItemDerivation(),
                new CustomerShipmentDerivation(),
                new OrderShipmentDerivation(),
                new SalesOrderItemsDerivation(), //Has Dependency on OrderShipment
                new SalesOrderDerivation(), //Has Dependency on SalesOrderItem

                new InvoiceItemsTotalIncVatDerivation(),
            };


            this.Session = database.CreateSession();

            if (populate)
            {
                Fixture.Setup(database, this.Config);
                this.Session.Commit();
            }
        }

        private Person GetPersonByUserName(string userName) => new People(this.Session).FindBy(M.User.UserName, userName);
    }
}
