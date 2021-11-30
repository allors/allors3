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

    public class AutoCompleteDerivedFilterTest : Test
    {
        public override void Configure(BrowserTypeLaunchOptions options) => options.Headless = false;

        public override void Configure(BrowserNewContextOptions options) => options.BaseURL = "http://localhost:4200";

        public FormPage FormPage => new FormPage(this.AppRoot);

        private Person john;
        private Person jane;
        private Person jenny;

        [SetUp]
        public async Task Setup()
        {
            this.john = new People(this.Transaction).FindBy(this.M.Person.UserName, "john@example.com");
            this.jane = new People(this.Transaction).FindBy(this.M.Person.UserName, "jane@example.com");
            this.jenny = new People(this.Transaction).FindBy(this.M.Person.UserName, "jenny@example.com");

            var singleton = this.Transaction.GetSingleton();
            singleton.AutocompleteDefault = this.jane;

            this.Transaction.Derive();
            this.Transaction.Commit();

            await this.Login("jane@example.com");
            await this.GotoAsync("/tests/form");
        }

        [Test]
        public async Task Empty()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormPage.SaveAsync();

            await this.Page.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.Null(data.AutocompleteAssignedFilter);
            Assert.AreEqual(this.jane, data.AutocompleteDerivedFilter);
        }

        [Test]
        public async Task UseInitialForAssigned()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormPage.AutocompleteDerivedFilter.SelectAsync("jane@example.com");

            await this.FormPage.SaveAsync();

            await this.Page.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.Null(data.AutocompleteAssignedFilter);
            Assert.AreEqual(this.jane, data.AutocompleteDerivedFilter);
        }

        [Test]
        public async Task UseOtherForAssigned()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormPage.AutocompleteDerivedFilter.SelectAsync("jenny@example.com");

            await this.FormPage.SaveAsync();

            await this.Page.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.AreEqual(this.jenny, data.AutocompleteAssignedFilter);
            Assert.AreEqual(this.jenny, data.AutocompleteDerivedFilter);
        }
    }
}
