// <copyright file="PersonEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.E2E.Objects
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.E2E.Angular;
    using Allors.E2E.Angular.Html;
    using Allors.E2E.Angular.Material.Factory;
    using Allors.E2E.Test;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class PurchaseOrderTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateMinimal()
        {
            var before = new PurchaseOrders(this.Transaction).Extent().ToArray();
            var supplier = new Organisations(this.Transaction).Extent().First(v => v.Name == "Allors BVBA").ActiveSuppliers.First();

            var @class = this.M.PurchaseOrder;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new PurchaseorderCreateFormComponent(this.OverlayContainer);

            await form.TakenViaSupplierAutocomplete.SelectAsync(supplier.Name);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new PurchaseOrders(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var purchaseOrder = after.Except(before).First();

            Assert.AreEqual(supplier, purchaseOrder.TakenViaSupplier);
        }

        [Test]
        public async Task CreateMaximum()
        {
            var before = new PurchaseOrders(this.Transaction).Extent().ToArray();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(v => v.Name == "Allors BVBA");
            var supplier = internalOrganisation.ActiveSuppliers.First();
            var supplierContactMechanism = supplier.CurrentPartyContactMechanisms.First().ContactMechanism;
            var takenViaContactPerson = supplier.CurrentContacts.First();
            var internalOrganisationContactPerson = internalOrganisation.CurrentContacts.First();
            var facility = internalOrganisation.FacilitiesWhereOwner.First();
            var vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;

            var @class = this.M.PurchaseOrder;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new PurchaseorderCreateFormComponent(this.OverlayContainer);

            await form.TakenViaSupplierAutocomplete.SelectAsync(supplier.Name);
            await form.DerivedTakenViaContactMechanismSelect.SelectAsync(supplierContactMechanism);
            await form.TakenViaContactPersonSelect.SelectAsync(takenViaContactPerson);
            await form.BillToContactPersonSelect.SelectAsync(internalOrganisationContactPerson);
            await form.ShipToContactPersonSelect.SelectAsync(internalOrganisationContactPerson);
            await form.StoredInFacilitySelect.SelectAsync(facility);
            await form.CustomerReferenceInput.SetValueAsync("CustomerReference");
            await form.DerivedVatRegimeSelect.SelectAsync(vatRegime);
            await form.DescriptionTextarea.SetAsync("Description");
            await form.InternalCommentTextarea.SetAsync("InternalComment");

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new PurchaseOrders(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var purchaseOrder = after.Except(before).First();

            Assert.AreEqual(supplier, purchaseOrder.TakenViaSupplier);
            Assert.AreEqual(supplierContactMechanism, purchaseOrder.DerivedTakenViaContactMechanism);
            Assert.AreEqual(takenViaContactPerson, purchaseOrder.TakenViaContactPerson);
            Assert.AreEqual(internalOrganisationContactPerson, purchaseOrder.BillToContactPerson);
            Assert.AreEqual(internalOrganisationContactPerson, purchaseOrder.ShipToContactPerson);
            Assert.AreEqual(facility, purchaseOrder.StoredInFacility);
            Assert.AreEqual("CustomerReference", purchaseOrder.CustomerReference);
            Assert.AreEqual(vatRegime, purchaseOrder.DerivedVatRegime);
            Assert.AreEqual("Description", purchaseOrder.Description);
            Assert.AreEqual("InternalComment", purchaseOrder.InternalComment);
        }
    }
}
