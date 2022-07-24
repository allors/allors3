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

    public class CatalogueTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateCatalogueMinimal()
        {
            //TODO: Koen

            var before = new Catalogues(this.Transaction).Extent().ToArray();
            var scope = new Scopes(this.Transaction).Public;

            var @class = this.M.Catalogue;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new CatalogueFormComponent(this.OverlayContainer);

            await form.CatScopeSelect.SelectAsync(scope);
            await form.NameInput.SetValueAsync("Joren");

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new Catalogues(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var catalogue = after.Except(before).First();

            Assert.AreEqual("Joren", catalogue.Name);
            Assert.AreEqual(scope, catalogue.CatScope);
        }

        [Test]
        public async Task CreateCatalogueMaximal()
        {
            //TODO: Catalogue Image

            var before = new Catalogues(this.Transaction).Extent().ToArray();
            var scope = new Scopes(this.Transaction).Public;
            var categorie = new ProductCategories(this.Transaction).Extent().First();

            var @class = this.M.Catalogue;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new CatalogueFormComponent(this.OverlayContainer);

            // Logo image
            await form.CatScopeSelect.SelectAsync(scope);
            await form.ProductCategoriesSelect.SelectAsync(categorie);
            await form.NameInput.SetValueAsync("Joren");

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new Catalogues(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var catalogue = after.Except(before).First();

            Assert.AreEqual("Joren", catalogue.Name);
            Assert.AreEqual(scope, catalogue.CatScope);
            Assert.AreEqual(categorie, catalogue.ProductCategories.First());
        }
    }
}
