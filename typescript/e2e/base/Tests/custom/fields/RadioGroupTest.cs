// <copyright file="AutoCompleteFilterTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Form
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.E2E.Angular.Material.Form;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class RadioGroupTest : Test
    {
        public FormComponent FormComponent => new FormComponent(this.AppRoot);

        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
            await this.GotoAsync("/fields");
        }

        [Test]
        public async Task SetFirst()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormComponent.RadioGroup.SelectAsync("one");

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(before.Length + 1, after.Length);
            var data = after.Except(before).First();
            Assert.AreEqual("one", data.RadioGroup);
        }

        [Test]
        public async Task SetSecond()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormComponent.RadioGroup.SelectAsync("two");

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(before.Length + 1, after.Length);
            var data = after.Except(before).First();
            Assert.AreEqual("two", data.RadioGroup);
        }
    }
}