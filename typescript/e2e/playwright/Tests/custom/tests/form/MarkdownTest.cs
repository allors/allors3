// <copyright file="AutoCompleteFilterTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Allors.Database.Domain;
    using Microsoft.Playwright;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class MarkdownTest : Test
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
        public async Task Populated()
        {
            var data = new DataBuilder(this.Transaction).Build();
            data.Markdown = "*** Hello ***";
            this.Transaction.Commit();

            await this.GotoAsync("/tests/form");

            var actual = await this.FormPage.Markdown.GetAsync();

            Assert.That(actual, Is.EqualTo("*** Hello ***"));
        }

        [Test]
        public async Task Set()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormPage.Markdown.SetAsync("*** Hello ***");

            await this.FormPage.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.AreEqual("*** Hello ***", data.Markdown);
        }
    }
}
