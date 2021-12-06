// <copyright file="AutoCompleteFilterTest.cs" company="Allors bvba">
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

    public class StaticTest : Test
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
        public async Task Populated()
        {
            var data = new DataBuilder(this.Transaction).Build();
            data.Static = "A Static String!";
            this.Transaction.Commit();

            await this.GotoAsync("/tests/form");

            var actual = await this.FormPage.Static.GetAsync();

            Assert.That(actual, Is.EqualTo("A Static String!"));
        }
    }
}
