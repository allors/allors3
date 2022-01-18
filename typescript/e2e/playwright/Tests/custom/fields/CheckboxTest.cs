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

    public class CheckboxTest : Test
    {
        public FormComponent FormComponent => new FormComponent(this.AppRoot);

        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
            await this.GotoAsync("/fields");
        }

        [Test]
        public async Task Indeterminate()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            var value = await this.FormComponent.Checkbox.GetAsync();

            Assert.Null(value);
        }

        [Test]
        public async Task SetTrue()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormComponent.Checkbox.SetAsync(true);

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.True(data.Checkbox);
        }

        [Test]
        public async Task SetFalse()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormComponent.Checkbox.SetAsync(false);

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.False(data.Checkbox);
        }
    }
}
