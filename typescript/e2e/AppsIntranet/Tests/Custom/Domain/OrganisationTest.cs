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

    public class OrganisationTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateOrganisationMinimal()
        {
            var before = new Organisations(this.Transaction).Extent().ToArray();

            var @class = this.M.Organisation;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new OrganisationCreateFormComponent(this.OverlayContainer);

            await form.NameInput.SetValueAsync("Driesjes");

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new Organisations(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var organisation = after.Except(before).First();

            Assert.AreEqual("Driesjes", organisation.Name);
        }

        [Test]
        public async Task CreateOrganisationMaximal()
        {
            var before = new Organisations(this.Transaction).Extent().ToArray();
            var legalForm = new LegalForms(this.Transaction).BeBvbaSprl;
            var locale = new Locales(this.Transaction).LocaleByName["en"];
            var organisationRole = new OrganisationRoles(this.Transaction).Supplier;
            var currency = new Currencies(this.Transaction).Extent().First();
            var industryClassification = new IndustryClassifications(this.Transaction).Extent().First();
            var customOrganisationClassification = new CustomOrganisationClassifications(this.Transaction).Extent().First();

            var @class = this.M.Organisation;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new OrganisationCreateFormComponent(this.OverlayContainer);

            await form.NameInput.SetValueAsync("Driesjes");
            await form.TaxNumberInput.SetValueAsync("BE047747474");
            await form.LegalFormSelect.SelectAsync(legalForm);
            await form.LocaleSelect.SelectAsync(locale);
            // TODO: Roles
            //await form.SelectAsync(organisationRole);
            await form.PreferredCurrencySelect.SelectAsync(currency);
            await form.IndustryClassificationsSelect.SelectAsync(industryClassification);
            await form.CustomClassificationsSelect.SelectAsync(customOrganisationClassification);
            await form.IsManufacturerSlideToggle.SetAsync(true);
            await form.IsInternalOrganisationSlideToggle.SetAsync(true);
            // TODO: Logo image
            await form.CommentTextarea.SetAsync("This is a comment");
            await form.CollectiveWorkEffortInvoiceSlideToggle.SetAsync(true);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new Organisations(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var organisation = after.Except(before).First();

            Assert.AreEqual("Driesjes", organisation.Name);
            Assert.AreEqual("BE047747474", organisation.TaxNumber);
            Assert.AreEqual(legalForm, organisation.LegalForm);
            Assert.AreEqual(locale, organisation.Locale);
            Assert.AreEqual(currency, organisation.PreferredCurrency);
            Assert.AreEqual(industryClassification, organisation.IndustryClassifications.First());
            Assert.AreEqual(customOrganisationClassification, organisation.CustomClassifications.First());
            Assert.AreEqual(true, organisation.IsManufacturer);
            Assert.AreEqual(true, organisation.IsInternalOrganisation);
            Assert.AreEqual("This is a comment", organisation.Comment);
            Assert.AreEqual(true, organisation.CollectiveWorkEffortInvoice);
        }
    }
}
