// <copyright file="AutoCompleteFilterTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.E2E.Form
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.E2E.Test;
    using E2E;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class AutoCompleteFilterTest : Test
    {
        public FieldsFormComponent FormComponent => new FieldsFormComponent(this.AppRoot);

        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
            await this.GotoAsync("/fields");
        }

        [Test]
        public async Task Full()
        {
            var jane = new People(this.Transaction).FindBy(this.M.Person.UserName, "jane@example.com");
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormComponent.AutocompleteFilterAutocomplete.SelectAsync("jane@example.com");

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.AreEqual(jane, data.AutocompleteFilter);
        }

        [Test]
        public async Task Partial()
        {
            var jane = new People(this.Transaction).FindBy(this.M.Person.UserName, "jane@example.com");
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormComponent.AutocompleteFilterAutocomplete.SelectAsync("j", "jane@example.com");

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.AreEqual(jane, data.AutocompleteFilter);
        }

        [Test]
        public async Task Blank()
        {
            var jane = new People(this.Transaction).FindBy(this.M.Person.UserName, "jane@example.com");
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormComponent.AutocompleteFilterAutocomplete.SelectAsync("", "jane@example.com");

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.AreEqual(jane, data.AutocompleteFilter);
        }
    }
}
