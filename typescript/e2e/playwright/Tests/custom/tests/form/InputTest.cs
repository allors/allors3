// <copyright file="InputTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Microsoft.Playwright;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class InputTest : Test
    {
        public override void Configure(BrowserTypeLaunchOptions options) => options.Headless = false;

        public override void Configure(BrowserNewContextOptions options) => options.BaseURL = "http://localhost:4200";

        public FormPage FormPage => new FormPage(this.AppRoot);

        [SetUp]
        public async Task Setup()
        {
            await this.Login("jane@example.com");
            await this.GotoAsync("/tests/form");
        }

        [Test]
        public async Task String()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormPage.String.SetAsync("Hello");
            await this.FormPage.SaveAsync();

            await this.Page.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.AreEqual("Hello", data.String);
        }

        [Test]
        public async Task Decimal()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormPage.Decimal.SetAsync(100.50m);
            await this.FormPage.SaveAsync();

            await this.Page.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.AreEqual(100.50m, data.Decimal);
        }
    }
}
