// <copyright file="ListPagesTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.ApplicationTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.E2E.Angular;
    using Allors.E2E.Angular.Material.Factory;
    using Allors.E2E.Angular.Material.Table;
    using Allors.E2E.Test;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class CountryTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task Create()
        {
            var before = new Countries(this.Transaction).Extent().ToArray();

            var @class = this.M.Country;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new CountryFormComponent(this.OverlayContainer);

            await form.NameInput.SetValueAsync("Alloristan");
            await form.IsoCodeInput.SetValueAsync("AS");

            var saveComponent = new SaveComponent(this.OverlayContainer);
            await saveComponent.SaveAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new Countries(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var country = after.Except(before).First();

            Assert.AreEqual("Alloristan", country.Name);
            Assert.AreEqual("AS", country.IsoCode);
        }

        [Test]
        public async Task Edit()
        {
            var country = new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "AL");

            var @class = this.M.Country;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var table = new AllorsMaterialTableComponent(this.AppRoot);
            await this.Page.WaitForAngular();
            await table.Action(country, "edit");
            await this.Page.WaitForAngular();

            var form = new CountryFormComponent(this.OverlayContainer);

            await form.NameInput.SetValueAsync("Albania Edit");
            await form.IsoCodeInput.SetValueAsync("AE");

            var saveComponent = new SaveComponent(this.OverlayContainer);
            await saveComponent.SaveAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            Assert.AreEqual("Albania Edit", country.Name);
            Assert.AreEqual("AE", country.IsoCode);
        }

    }
}
