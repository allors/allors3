// <copyright file="PersonEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Objects
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
        public async Task CreateMinimal()
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

            var person = after.Except(before).First();

            Assert.AreEqual("Driesjes", person.Name);
        }

        [Test]
        public async Task CreateMaximum()
        {
            var before = new Organisations(this.Transaction).Extent().ToArray();
            var legalForm = new LegalForms(this.Transaction).BeBvbaSprl;
            var locale = new Locales(this.Transaction).EnglishGreatBritain;
            var organisationRole = new OrganisationRoles(this.Transaction).Supplier;
            var currency = new Currencies(this.Transaction).Extent().First();

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
            //await form..SelectAsync(organisationRole); (roles)
            await form.PreferredCurrencySelect.SelectAsync(currency);
            // Industries
            // Classifications
            await form.IsManufacturerSlideToggle.SetAsync(true);
            await form.IsInternalOrganisationSlideToggle.SetAsync(true);
            // Logo image
            await form.CommentTextarea.SetAsync("This is a comment");
            await form.CollectiveWorkEffortInvoiceSlideToggle.SetAsync(true);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new Organisations(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var person = after.Except(before).First();

            Assert.AreEqual("Driesjes", person.Name);
        }
    }
}
