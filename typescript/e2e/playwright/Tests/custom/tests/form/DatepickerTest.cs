// <copyright file="AutoCompleteFilterTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Globalization;
    using System.Linq;
    using Allors.Database.Domain;
    using Microsoft.Playwright;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class DatepickerTest : Test
    {
        public override void Configure(BrowserTypeLaunchOptions options) => options.Headless = false;

        public override void Configure(BrowserNewContextOptions options) => options.BaseURL = "http://localhost:4200";

        public FormPage FormPage => new FormPage(this.AppRoot);

        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
            await this.GotoAsync("/tests/form");
        }

        [Test]
        public async Task Initial()
        {
            CultureInfo.CurrentCulture = new CultureInfo("nl-BE");

            var date = await this.FormPage.Date.GetAsync();

            Assert.IsNull(date);
        }

        [Test]
        public async Task SetDate()
        {
            CultureInfo.CurrentCulture = new CultureInfo("nl-BE");

            var before = new Datas(this.Transaction).Extent().ToArray();

            var now = this.Transaction.Now();

            await this.FormPage.Date.SetAsync(now);

            await this.FormPage.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(before.Length + 1, after.Length);
            var data = after.Except(before).First();
            Assert.True(data.Date != null);
            Assert.AreEqual(now.Year, data.Date.Value.Year);
            Assert.AreEqual(now.Month, data.Date.Value.Month);
            Assert.AreEqual(now.Day, data.Date.Value.Day);
        }
    }
}
