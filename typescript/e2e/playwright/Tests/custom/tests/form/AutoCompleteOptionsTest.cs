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

    public class AutoCompleteOptionsTest : Test
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
        public async Task Full()
        {
            var jane = new People(this.Transaction).FindBy(this.M.Person.UserName, "jane@example.com");
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormPage.AutocompleteOptions.SelectAsync("jane@example.com", "jane@example.com");

            await this.FormPage.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.AreEqual(jane, data.AutocompleteOptions);
        }

        [Test]
        public async Task Partial()
        {
            var jane = new People(this.Transaction).FindBy(this.M.Person.UserName, "jane@example.com");
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormPage.AutocompleteOptions.SelectAsync("j", "jane@example.com");

            await this.FormPage.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.AreEqual(jane, data.AutocompleteOptions);
        }

        [Test]
        public async Task Blank()
        {
            var jane = new People(this.Transaction).FindBy(this.M.Person.UserName, "jane@example.com");
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormPage.AutocompleteOptions.SelectAsync("", "jane@example.com");

            await this.FormPage.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.AreEqual(jane, data.AutocompleteOptions);
        }
    }
}
