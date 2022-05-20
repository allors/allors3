// <copyright file="InputTest.cs" company="Allors bvba">
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

    public class AutoCompleteDerivedFilterTest : Test
    {
        public FieldsFormComponent FormComponent => new FieldsFormComponent(this.AppRoot);

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

            await this.LoginAsync("jane@example.com");
            await this.Page.NavigateAsync("/fields");
        }

        [Test]
        public async Task Empty()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormComponent.SaveAsync();
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

            await this.FormComponent.AutocompleteDerivedFilterAutocomplete.SelectAsync("jane@example.com");

            await this.FormComponent.SaveAsync();
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

            await this.FormComponent.AutocompleteDerivedFilterAutocomplete.SelectAsync("jenny@example.com");

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.AreEqual(this.jenny, data.AutocompleteAssignedFilter);
            Assert.AreEqual(this.jenny, data.AutocompleteDerivedFilter);
        }
    }
}
