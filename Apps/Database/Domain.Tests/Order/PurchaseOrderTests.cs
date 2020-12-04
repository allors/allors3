// <copyright file="PurchaseOrderTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using TestPopulation;
    using Resources;
    using Xunit;

    public class PurchaseOrderTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPurchaseOrderBuilder_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            var internalOrganisation = this.InternalOrganisation;
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            var order = new PurchaseOrderBuilder(this.Session).WithTakenViaSupplier(supplier).Build();

            this.Session.Derive();

            Assert.Equal(new PurchaseOrderStates(this.Session).Created, order.PurchaseOrderState);
            Assert.Equal(this.Session.Now().Date, order.OrderDate.Date);
            Assert.Equal(this.Session.Now().Date, order.EntryDate.Date);
            Assert.Equal(order.PreviousTakenViaSupplier, order.TakenViaSupplier);
        }

        [Fact]
        public void GivenOrder_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("customer2").Build();
            var internalOrganisation = this.InternalOrganisation;
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            ContactMechanism takenViaContactMechanism = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var supplierContactMechanism = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(takenViaContactMechanism)
                .WithUseAsDefault(true)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).OrderAddress)
                .Build();
            supplier.AddPartyContactMechanism(supplierContactMechanism);

            this.Session.Derive();
            this.Session.Commit();

            var builder = new PurchaseOrderBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithTakenViaSupplier(supplier);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            builder.WithAssignedTakenViaContactMechanism(takenViaContactMechanism);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenPurchaseOrder_WhenDeriving_ThenTakenViaSupplierMustBeInSupplierRelationship()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("customer2").Build();
            var internalOrganisation = this.InternalOrganisation;

            new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(supplier)
                .Build();

            var expectedError = ErrorMessages.PartyIsNotASupplier;
            Assert.Equal(expectedError, this.Session.Derive(false).Errors[0].Message);

            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOrder_WhenDeriving_ThenLocaleMustExist()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("customer2").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            ContactMechanism takenViaContactMechanism = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var supplierContactMechanism = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(takenViaContactMechanism)
                .WithUseAsDefault(true)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).OrderAddress)
                .Build();
            supplier.AddPartyContactMechanism(supplierContactMechanism);

            var order = new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(supplier)
                .Build();

            this.Session.Derive();

            Assert.Equal(order.OrderedBy.Locale, order.DerivedLocale);
        }

        [Fact]
        public void GivenPurchaseOrder_WhenGettingOrderNumberWithoutFormat_ThenOrderNumberShouldBeReturned()
        {
            var internalOrganisation = this.InternalOrganisation;
            internalOrganisation.RemovePurchaseOrderNumberPrefix();

            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var order1 = new PurchaseOrderBuilder(this.Session).WithTakenViaSupplier(supplier).Build();

            this.Session.Derive();

            Assert.Equal("1", order1.OrderNumber);

            var order2 = new PurchaseOrderBuilder(this.Session).WithTakenViaSupplier(supplier).Build();

            this.Session.Derive();

            Assert.Equal("2", order2.OrderNumber);
        }

        [Fact]
        public void GivenPurchaseOrder_WhenGettingOrderNumberWithFormat_ThenFormattedOrderNumberShouldBeReturned()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var internalOrganisation = this.InternalOrganisation;
            internalOrganisation.PurchaseOrderNumberPrefix = "the format is ";

            var order1 = new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(supplier)
                .Build();

            this.Session.Derive();

            Assert.Equal("the format is 1", order1.OrderNumber);

            var order2 = new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(supplier)
                .Build();

            this.Session.Derive();

            Assert.Equal("the format is 2", order2.OrderNumber);
        }

        [Fact]
        public void GivenBilledToWithoutOrderNumberPrefix_WhenDeriving_ThenSortableOrderNumberIsSet()
        {
            this.InternalOrganisation.RemovePurchaseOrderNumberPrefix();
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var order = new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(supplier)
                .Build();

            order.SetReadyForProcessing();
            this.Session.Derive();

            Assert.Equal(int.Parse(order.OrderNumber), order.SortableOrderNumber);
        }

        [Fact]
        public void GivenBilledToWithOrderNumberPrefix_WhenDeriving_ThenSortableOrderNumberIsSet()
        {
            this.InternalOrganisation.PurchaseOrderNumberPrefix = "prefix-";
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var order = new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(supplier)
                .Build();

            order.SetReadyForProcessing();
            this.Session.Derive();

            Assert.Equal(int.Parse(order.OrderNumber.Split('-')[1]), order.SortableOrderNumber);
        }

        [Fact]
        public void GivenBilledToWithParametrizedOrderNumberPrefix_WhenDeriving_ThenSortableOrderNumberIsSet()
        {
            this.InternalOrganisation.PurchaseOrderNumberPrefix = "prefix-{year}-";
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var order = new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(supplier)
                .Build();

            order.SetReadyForProcessing();
            this.Session.Derive();

            Assert.Equal(int.Parse(string.Concat(this.Session.Now().Date.Year.ToString(), order.OrderNumber.Split('-').Last())), order.SortableOrderNumber);
        }

        [Fact]
        public void GivenPurchaseOrder_WhenConfirming_ThenAllValidItemsAreInConfirmedState()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("customer2").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            var part = new NonUnifiedPartBuilder(this.Session)
                .WithProductIdentification(new PartNumberBuilder(this.Session)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build())
                .Build();

            var order = new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(supplier)
                .WithAssignedVatRegime(new VatRegimes(this.Session).Exempt)
                .Build();

            var item1 = new PurchaseOrderItemBuilder(this.Session).WithPart(part).WithQuantityOrdered(1).Build();
            var item2 = new PurchaseOrderItemBuilder(this.Session).WithPart(part).WithQuantityOrdered(2).Build();
            var item3 = new PurchaseOrderItemBuilder(this.Session).WithPart(part).WithQuantityOrdered(3).Build();
            var item4 = new PurchaseOrderItemBuilder(this.Session).WithPart(part).WithQuantityOrdered(4).Build();
            order.AddPurchaseOrderItem(item1);
            order.AddPurchaseOrderItem(item2);
            order.AddPurchaseOrderItem(item3);
            order.AddPurchaseOrderItem(item4);

            this.Session.Derive();

            order.SetReadyForProcessing();
            this.Session.Derive();

            item4.Cancel();
            this.Session.Derive();

            Assert.Equal(3, order.ValidOrderItems.Count);
            Assert.Contains(item1, order.ValidOrderItems);
            Assert.Contains(item2, order.ValidOrderItems);
            Assert.Contains(item3, order.ValidOrderItems);
            Assert.Equal(new PurchaseOrderItemStates(this.Session).InProcess, item1.PurchaseOrderItemState);
            Assert.Equal(new PurchaseOrderItemStates(this.Session).InProcess, item2.PurchaseOrderItemState);
            Assert.Equal(new PurchaseOrderItemStates(this.Session).InProcess, item3.PurchaseOrderItemState);
            Assert.Equal(new PurchaseOrderItemStates(this.Session).Cancelled, item4.PurchaseOrderItemState);
        }

        [Fact]
        public void GivenPurchaseOrder_WhenOrdering_ThenAllValidItemsAreInInProcessState()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("customer2").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            var part = new NonUnifiedPartBuilder(this.Session)
                .WithProductIdentification(new PartNumberBuilder(this.Session)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build())
                .Build();

            var order = new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(supplier)
                .WithAssignedVatRegime(new VatRegimes(this.Session).Exempt)
                .Build();

            var item1 = new PurchaseOrderItemBuilder(this.Session).WithPart(part).WithQuantityOrdered(1).Build();
            var item2 = new PurchaseOrderItemBuilder(this.Session).WithPart(part).WithQuantityOrdered(2).Build();
            var item3 = new PurchaseOrderItemBuilder(this.Session).WithPart(part).WithQuantityOrdered(3).Build();
            order.AddPurchaseOrderItem(item1);
            order.AddPurchaseOrderItem(item2);
            order.AddPurchaseOrderItem(item3);

            this.Session.Derive();

            order.SetReadyForProcessing();
            this.Session.Derive();

            Assert.Equal(3, order.ValidOrderItems.Count);
            Assert.Contains(item1, order.ValidOrderItems);
            Assert.Contains(item2, order.ValidOrderItems);
            Assert.Contains(item3, order.ValidOrderItems);
            Assert.Equal(new PurchaseOrderItemStates(this.Session).InProcess, item1.PurchaseOrderItemState);
            Assert.Equal(new PurchaseOrderItemStates(this.Session).InProcess, item2.PurchaseOrderItemState);
            Assert.Equal(new PurchaseOrderItemStates(this.Session).InProcess, item3.PurchaseOrderItemState);
        }
    }

    [Trait("Category", "Security")]
    public class PurchaseOrderSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderSecurityTests(Fixture fixture) : base(fixture)
        {
            this.deletePermission = new Permissions(this.Session).Get(this.M.PurchaseOrder.ObjectType, this.M.PurchaseOrder.Delete);
            this.setReadyPermission = new Permissions(this.Session).Get(this.M.PurchaseOrder.ObjectType, this.M.PurchaseOrder.SetReadyForProcessing);
            this.invoicePermission = new Permissions(this.Session).Get(this.M.PurchaseOrder.ObjectType, this.M.PurchaseOrder.Invoice);
            this.revisePermission = new Permissions(this.Session).Get(this.M.PurchaseOrder.ObjectType, this.M.PurchaseOrder.Revise);
            this.quickReceivePermission = new Permissions(this.Session).Get(this.M.PurchaseOrder.ObjectType, this.M.PurchaseOrder.QuickReceive);
            this.rejectPermission = new Permissions(this.Session).Get(this.M.PurchaseOrder.ObjectType, this.M.PurchaseOrder.Reject);
            this.cancelPermission = new Permissions(this.Session).Get(this.M.PurchaseOrder.ObjectType, this.M.PurchaseOrder.Cancel);
        }

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;
        private readonly Permission setReadyPermission;
        private readonly Permission invoicePermission;
        private readonly Permission revisePermission;
        private readonly Permission quickReceivePermission;
        private readonly Permission rejectPermission;
        private readonly Permission cancelPermission;


        [Fact]
        public void OnChangedPurchaseOrderStateIsCompletedDeriveInvoicePermission()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var purchaseOrderItem = new PurchaseOrderItemBuilder(this.Session)
                .WithInvoiceItemType(new InvoiceItemTypeBuilder(this.Session).Build())
                .WithAssignedUnitPrice(1)
            .Build();
            purchaseOrder.AddPurchaseOrderItem(purchaseOrderItem);
            this.Session.Derive(false);

            purchaseOrder.Approve();
            this.Session.Derive(false);

            purchaseOrder.Send();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.invoicePermission, purchaseOrder.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateApproveDeriveInvoicePermission()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var purchaseOrderItem = new PurchaseOrderItemBuilder(this.Session)
                .WithInvoiceItemType(new InvoiceItemTypeBuilder(this.Session).Build())
                .WithAssignedUnitPrice(1)
            .Build();
            purchaseOrder.AddPurchaseOrderItem(purchaseOrderItem);
            this.Session.Derive(false);

            purchaseOrder.Approve();
            this.Session.Derive(false);

            Assert.Contains(this.invoicePermission, purchaseOrder.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateIsCompletedWithoutValidItemsDeriveInvoicePermission()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            purchaseOrder.Approve();
            this.Session.Derive(false);

            purchaseOrder.Send();
            this.Session.Derive(false);

            Assert.Contains(this.invoicePermission, purchaseOrder.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateIsCompletedDeriveRevisePermission()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            purchaseOrder.Approve();
            this.Session.Derive(false);

            purchaseOrder.Send();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.revisePermission, purchaseOrder.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateCreatedDeriveRevisePermission()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Contains(this.revisePermission, purchaseOrder.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateIsCompletedWithPurchaseInvoiceDeriveRevisePermission()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var purchaseOrderItem = new PurchaseOrderItemBuilder(this.Session)
                .WithInvoiceItemType(new InvoiceItemTypeBuilder(this.Session).Build())
                .WithAssignedUnitPrice(1)
            .Build();
            purchaseOrder.AddPurchaseOrderItem(purchaseOrderItem);
            this.Session.Derive(false);

            purchaseOrder.Approve();
            this.Session.Derive(false);

            purchaseOrder.Send();
            this.Session.Derive(false);

            purchaseOrder.Invoice();
            this.Session.Derive(false);

            Assert.Contains(this.revisePermission, purchaseOrder.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateSentDeriveQuickReceivePermission()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var part = new NonUnifiedPartBuilder(this.Session).WithNonSerialisedDefaults(this.InternalOrganisation).Build();

            var purchaseOrderItem = new PurchaseOrderItemBuilder(this.Session)
                .WithNonSerializedPartDefaults(part)
                .Build();

            purchaseOrder.AddPurchaseOrderItem(purchaseOrderItem);
            this.Session.Derive(false);

            purchaseOrder.Approve();
            this.Session.Derive(false);

            purchaseOrder.Send();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.quickReceivePermission, purchaseOrder.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateCreatedDeriveQuickReceivePermission()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var purchaseOrderItem = new PurchaseOrderItemBuilder(this.Session)
                .WithIsReceivable(true)
                .WithInvoiceItemType(new InvoiceItemTypeBuilder(this.Session).Build())
                .WithAssignedUnitPrice(1)
            .Build();
            purchaseOrder.AddPurchaseOrderItem(purchaseOrderItem);
            this.Session.Derive(false);

            purchaseOrder.Approve();
            this.Session.Derive(false);

            Assert.Contains(this.quickReceivePermission, purchaseOrder.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateSentWithoutRecievableItemsDeriveQuickReceivePermission()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var purchaseOrderItem = new PurchaseOrderItemBuilder(this.Session)
                .WithIsReceivable(false)
                .WithInvoiceItemType(new InvoiceItemTypeBuilder(this.Session).Build())
                .WithAssignedUnitPrice(1)
            .Build();
            purchaseOrder.AddPurchaseOrderItem(purchaseOrderItem);
            this.Session.Derive(false);

            purchaseOrder.Approve();
            this.Session.Derive(false);

            purchaseOrder.Send();
            this.Session.Derive(false);

            Assert.Contains(this.quickReceivePermission, purchaseOrder.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateCreatedDeriveDeletePermission()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, purchaseOrder.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateSentDeriveDeletePermission()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            purchaseOrder.Approve();
            this.Session.Derive(false);

            purchaseOrder.Send();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, purchaseOrder.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateSentWithPurchaseInvoiceDeriveDeletePermission()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            purchaseOrder.Approve();
            this.Session.Derive(false);

            purchaseOrder.Send();
            this.Session.Derive(false);

            purchaseOrder.Invoice();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, purchaseOrder.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateCreatedWithSerialisedItemDeriveDeletePermission()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).WithDefaults(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            var serializedPart = new UnifiedGoodBuilder(this.Session).WithSerialisedDefaults(this.InternalOrganisation).Build();
            var serializedItem = new SerialisedItemBuilder(this.Session).WithDefaults(this.InternalOrganisation).Build();
            serializedPart.AddSerialisedItem(serializedItem);

            this.Session.Derive(false);

            var purchaseOrderItem = new PurchaseOrderItemBuilder(this.Session).WithSerializedPartDefaults(serializedPart, serializedItem).Build();
            purchaseOrder.AddPurchaseOrderItem(purchaseOrderItem);

            purchaseOrder.Send();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, purchaseOrder.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateCreatedWithWorkEffortPurchaseOrderItemAssignmentDeriveDeletePermission()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var serializedPart = new UnifiedGoodBuilder(this.Session).WithSerialisedDefaults(this.InternalOrganisation).Build();
            var serializedItem = new SerialisedItemBuilder(this.Session).WithDefaults(this.InternalOrganisation).Build();
            serializedPart.AddSerialisedItem(serializedItem);

            this.Session.Derive(false);

            var purchaseOrderItem = new PurchaseOrderItemBuilder(this.Session).WithSerializedPartDefaults(serializedPart, serializedItem).Build();
            purchaseOrder.AddPurchaseOrderItem(purchaseOrderItem);

            var workEffortPurchaseOrderItemAssignments = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Session)
                .WithAssignment(new WorkTaskBuilder(this.Session).Build())
                .WithPurchaseOrderItem(purchaseOrderItem)
                .WithPurchaseOrder(purchaseOrder)
                .Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, purchaseOrder.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateCreatedWithNotDeletablePurchaseOrderItemsDeriveDeletePermission()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var purchaseOrderItem = new PurchaseOrderItemBuilder(this.Session)
                .WithInvoiceItemType(new InvoiceItemTypeBuilder(this.Session).Build())
                .WithAssignedUnitPrice(1)
            .Build();
            purchaseOrder.AddPurchaseOrderItem(purchaseOrderItem);
            this.Session.Derive(false);

            purchaseOrderItem.Approve();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, purchaseOrder.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateCreatedPurchaseOrderShipmentStateIsNaDeriveMultiplePermissions()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var part = new NonUnifiedPartBuilder(this.Session).WithNonSerialisedDefaults(this.InternalOrganisation).Build();

            var purchaseOrderItem = new PurchaseOrderItemBuilder(this.Session)
                .WithNonSerializedPartDefaults(part)
                .Build();

            purchaseOrder.AddPurchaseOrderItem(purchaseOrderItem);

            var shipmentReceipt = new ShipmentReceiptBuilder(this.Session)
                .WithOrderItem(purchaseOrderItem)
                .WithQuantityAccepted(1)
                .Build();

            this.Session.Derive(false);

            Assert.Contains(this.rejectPermission, purchaseOrder.DeniedPermissions);
            Assert.Contains(this.cancelPermission, purchaseOrder.DeniedPermissions);
            Assert.Contains(this.quickReceivePermission, purchaseOrder.DeniedPermissions);
            Assert.Contains(this.revisePermission, purchaseOrder.DeniedPermissions);
            Assert.Contains(this.setReadyPermission, purchaseOrder.DeniedPermissions);
        }
    }

    [Trait("Category", "Approval")]
    public class PurchaseOrderApprovalDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderApprovalDerivationTests(Fixture fixture) : base(fixture)
        {

        }

        [Fact]
        public void OnCreatedPurchaseOrderApprovalLevel1DeriveTitle()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).WithDefaults(this.InternalOrganisation).Build();

            this.Session.Derive(false);

            var approval = new PurchaseOrderApprovalLevel1Builder(this.Session).WithPurchaseOrder(purchaseOrder).Build();

            this.Session.Derive(false);

            Assert.Equal(approval.Title, "Approval of " + purchaseOrder.WorkItemDescription);
        }

        [Fact]
        public void OnCreatedPurchaseOrderApprovalLevel1DeriveWorkItem()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).WithDefaults(this.InternalOrganisation).Build();

            this.Session.Derive(false);

            var approval = new PurchaseOrderApprovalLevel1Builder(this.Session).WithPurchaseOrder(purchaseOrder).Build();

            this.Session.Derive(false);

            Assert.Equal(approval.WorkItem, purchaseOrder);
        }

        [Fact]
        public void OnCreatedPurchaseOrderApprovalLevel1DeriveDateClosedExists()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).WithDefaults(this.InternalOrganisation).Build();

            this.Session.Derive(false);

            var approval = new PurchaseOrderApprovalLevel1Builder(this.Session).WithPurchaseOrder(purchaseOrder).Build();

            this.Session.Derive(false);

            Assert.True(approval.ExistDateClosed);
        }

        [Fact]
        public void OnCreatedPurchaseOrdeApprovalLevel1DeriveDateClosedNotExists()
        {
            var purchaseOrder = this.InternalOrganisation.CreatePurchaseOrderWithBothItems(this.Session.Faker());

            var supplierRelationship = purchaseOrder.TakenViaSupplier.SupplierRelationshipsWhereSupplier.First(v => v.InternalOrganisation == purchaseOrder.OrderedBy);
            supplierRelationship.NeedsApproval = true;
            supplierRelationship.ApprovalThresholdLevel1 = 1;

            this.Session.Derive(false);

            purchaseOrder.SetReadyForProcessing();

            this.Session.Derive(false);

            Assert.False(purchaseOrder.PurchaseOrderApprovalsLevel1WherePurchaseOrder.First().ExistDateClosed);
        }

        [Fact]
        public void OnCreatedPurchaseOrderApprovalLevel1DeriveEmptyParticipants()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).WithDefaults(this.InternalOrganisation).Build();

            this.Session.Derive(false);

            var approval = new PurchaseOrderApprovalLevel1Builder(this.Session).WithPurchaseOrder(purchaseOrder).Build();

            this.Session.Derive(false);

            Assert.Empty(approval.Participants);
        }

        /*Level2 Tests*/

        [Fact]
        public void OnCreatedPurchaseOrderApprovalLevel2DeriveTitle()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).WithDefaults(this.InternalOrganisation).Build();

            this.Session.Derive(false);

            var approval = new PurchaseOrderApprovalLevel2Builder(this.Session).WithPurchaseOrder(purchaseOrder).Build();

            this.Session.Derive(false);

            Assert.Equal(approval.Title, "Approval of " + purchaseOrder.WorkItemDescription);
        }
    }
}
