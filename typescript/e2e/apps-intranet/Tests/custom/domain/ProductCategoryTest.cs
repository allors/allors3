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

    public class ProductCategoryTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateMinimal()
        {
            var before = new ProductCategories(this.Transaction).Extent().ToArray();
            var scope = new Scopes(this.Transaction).Private;

            var @class = this.M.ProductCategory;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new ProductcategoryFormComponent(this.OverlayContainer);

            await form.NameInput.SetValueAsync("Lucca");
            await form.CatScopeSelect.SelectAsync(scope);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new ProductCategories(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var productCategory = after.Except(before).First();

            Assert.AreEqual("Lucca", productCategory.Name);
            Assert.AreEqual(scope, productCategory.CatScope);
        }

        [Test]
        public async Task CreateMaximum()
        {
            var before = new ProductCategories(this.Transaction).Extent().ToArray();
            var scope = new Scopes(this.Transaction).Private;
            var primaryParent = new ProductCategories(this.Transaction).Extent().First();
            var secondaryParent = new ProductCategories(this.Transaction).Extent().First();

            var @class = this.M.ProductCategory;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new ProductcategoryFormComponent(this.OverlayContainer);

            await form.NameInput.SetValueAsync("Lucca");
            await form.CatScopeSelect.SelectAsync(scope);
            await form.PrimaryParentSelect.SelectAsync(primaryParent);
            await form.SecondaryParentsSelect.SelectAsync(secondaryParent);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new ProductCategories(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var productCategory = after.Except(before).First();

            Assert.AreEqual("Lucca", productCategory.Name);
            Assert.AreEqual(scope, productCategory.CatScope);
            Assert.AreEqual(primaryParent, productCategory.PrimaryParent);
            Assert.AreEqual(1, productCategory.SecondaryParents.Count());
            Assert.Contains(secondaryParent, productCategory.SecondaryParents.ToArray());
        }
    }
}
