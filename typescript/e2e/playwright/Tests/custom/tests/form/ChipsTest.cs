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

    public class ChipsTest : Test
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
        public async Task AddOne()
        {
            var jane = new People(this.Transaction).FindBy(this.M.Person.UserName, "jane@example.com");
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormPage.Chips.AddAsync("jane", "jane@example.com");

            await this.FormPage.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.Contains(jane, data.Chips.ToArray());
        }

        [Test]
        public async Task AddTwo()
        {
            var jane = new People(this.Transaction).FindBy(this.M.Person.UserName, "jane@example.com");
            var john = new People(this.Transaction).FindBy(this.M.Person.UserName, "john@example.com");
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormPage.Chips.AddAsync("jane", "jane@example.com");
            await this.FormPage.Chips.AddAsync("john", "john@example.com");

            await this.FormPage.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.Contains(jane, data.Chips.ToArray());
            Assert.Contains(john, data.Chips.ToArray());
        }

        [Test]
        public async Task RemoveOne()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormPage.Chips.AddAsync("jane", "jane@example.com");

            await this.FormPage.SaveAsync();

            await this.FormPage.Chips.RemoveAsync("jane@example.com");

            await this.FormPage.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.IsEmpty(data.Chips);
        }
    }
}