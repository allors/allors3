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

            
            database.AuditableExtensionsRegisterDerivations();
            database.ShipmentExtensionsRegisterDerivations();
            database.SupplierOfferingRegisterDerivations();
            database.ServiceExtensionsRegisterDerivations();
            database.SerialisedItemCharacteristicRegisterDerivations();
            database.SerialisedInventoryItemRegisterDerivations();
            database.ProductCategoryRegisterDerivations();
            database.PriceComponentExtensionsRegisterDerivations();
            database.PartCategoryRegisterDerivations();
            database.OrderValueRegisterDerivations();
            database.OrderQuantityBreakRegisterDerivations();
            database.NonUnifiedGoodRegisterDerivations();
            database.StatementOfWorkRegisterDerivations();
            database.SurchargeComponentRegisterDerivations();
            database.SerialisedItemRegisterDerivations();
            database.QuoteItemRegisterDerivations();
            database.PurchaseOrderItemDerivations();
            database.PurchaseOrderApprovalLevel1RegisterDerivations();
            database.PropasalRegisterDerivations();
            database.ProductQuoteApprovalRegisterDerivations();
            database.ProductQuoteRegisterDerivations();
            database.OrderAdjustmentRegisterDerivations();
            database.EngagementRegisterDerivations();
            database.SalesOrderTransferRegisterDerivations();
            database.QuoteExtensionsRegisterDerivations();
            database.PurchaseOrderApprovalLevel2RegisterDerivations();
            database.PurchaseReturnRegisterDerivations();
            database.PurchaseShipmentRegisterDerivations();
            database.ShipmentPackageRegisterDerivations();
            database.ShipmentValueRegisterDerivations();
            database.PickListItemRegisterDerivations();
            database.PickListRegisterDerivations();
            database.PackagingContentRegisterDerivations();
            database.DropShipmentsRegisterDerivations();
            database.TransferRegisterDerivations();
            database.shipmentReceiptRegisterDerivations();
            database.CustomerReturnRegisterDerivations();
            database.SalesOrderItemInventoryAssignmentRegisterDerivations();
            database.UnifiedGoodRegisterDerivations();
            database.NonSerialisedInventoryItemRegisterDerivations();
            database.NonUnifiedPartRegisterDerivations();
            database.PartExtensionsRegisterDerivations();
            database.InventoryItemTransactionRegisterDerivations();
            database.InventoryItemExtensionsRegisterDerivations();
            database.CatalogueRegisterDerivations();
            database.PriceComponentRegisterDerivations();
            database.SingletonRegisterDerivations();
            database.SettingsRegisterDerivations();
            database.PhoneCommunicationRegisterDerivations();
            database.ProfessionalServicesRelationshipRegisterDerivations();
            database.OrganisationContactRelationshipRegisterDerivations();
            database.InternalOrganisationExtensionsRegisterDerivations();
            database.FaceToFaceCommunicationRegisterDerivations();
            database.EmploymentRegisterDerivations();
            database.CommunicationTaskRegisterDerivations();
            database.CommunicationEventExtensionsRegisterDerivations();
            database.AutomatedAgentRegisterDerivations();
            database.AgreementTermExtensionsRegisterDerivations();
            database.EmailCommunicationRegisterDerivations();
            database.OrganisationRegisterDerivations();
            database.PersonRegisterDerivations();
            database.MediaRegisterDerivations();
            database.PartyRegisterDerivations();
            database.EmailTemplateRegisterDerivations();
            database.WebSiteCommunicationsRegisterDerivations();
            database.CustomerRelationshipRegisterDerivations();
            database.FaxCommunicationRegisterDerivations();
            database.LetterCorrespondenceRegisterDerivations();
            database.OrganisationRollUpRegisterDerivations();
            database.PartyContactMechanismRegisterDerivations();
            database.subcontractorRelationshipRegisterDerivations();
            database.PassportRegisterDerivations();
            database.RequestItemRegisterDerivations();
            database.RequestForQuoteRegisterDerivations();
            database.RequestForProposalRegisterDerivations();
            database.RequestForInformationRegisterDerivations();
            database.RequestExtensionsRegisterDerivations();
            database.PartyFinancialRelationshipRegisterDerivations();
            database.SalesInvoiceRegisterDerivations();

            database.SupplierRelationshipRegisterDerivations();
            database.PurchaseOrderRegisterDerivations();  //Has Dependency on SupplierRelationship

            database.ShipmentItemRegisterDerivations();
            database.CustomerShipmentRegisterDerivations();

            database.OrderShipmentRegisterDerivations();
            database.SalesOrderItemsRegisterDerivations(); //Has Dependency on OrderShipment
            database.SalesOrderRegisterDerivations();   //Has Dependency on SalesOrderItem

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
