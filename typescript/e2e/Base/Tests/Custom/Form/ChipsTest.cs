// <copyright file="AutoCompleteFilterTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.E2E.Form
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.E2E.Angular;
    using Allors.E2E.Test;
    using E2E;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class ChipsTest : Test
    {
        public FieldsFormComponent FormComponent => new FieldsFormComponent(this.AppRoot);

        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
            await this.Page.NavigateAsync("/fields");
        }

        [Test]
        public async Task AddOne()
        {
            var jane = new People(this.Transaction).FindBy(this.M.Person.UserName, "jane@example.com");
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormComponent.ChipsChips.AddAsync("jane", "jane@example.com");

            await this.FormComponent.SaveAsync();
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

            await this.FormComponent.ChipsChips.AddAsync("jane", "jane@example.com");
            await this.FormComponent.ChipsChips.AddAsync("john", "john@example.com");

            await this.FormComponent.SaveAsync();
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

            await this.FormComponent.ChipsChips.AddAsync("jane", "jane@example.com");

            await this.FormComponent.SaveAsync();

            await this.FormComponent.ChipsChips.RemoveAsync("jane@example.com");

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.IsEmpty(data.Chips);
        }
    }
}
