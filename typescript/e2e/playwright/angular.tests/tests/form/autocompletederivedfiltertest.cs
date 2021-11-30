// <copyright file="AutoCompleteFilterTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Components;
    using src.app.tests.form;
    using Xunit;

    [Collection("Test collection")]
    public class AutoCompleteDerivedFilterTest : Test
    {
        private readonly FormComponent page;

        private readonly Person john;
        private readonly Person jane;
        private readonly Person jenny;

        public AutoCompleteDerivedFilterTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.page = this.Sidenav.NavigateToForm();

            this.john = new People(this.Transaction).FindBy(this.M.Person.UserName, "john@example.com");
            this.jane = new People(this.Transaction).FindBy(this.M.Person.UserName, "jane@example.com");
            this.jenny = new People(this.Transaction).FindBy(this.M.Person.UserName, "jenny@example.com");

            var singleton = this.Transaction.GetSingleton();
            // TODO:
            //singleton.AutocompleteDefault = john;

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        [Fact]
        public void Empty()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            this.page.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var data = after.Except(before).First();

            Assert.Null(data.AutocompleteAssignedFilter);
            Assert.Equal(this.john, data.AutocompleteDerivedFilter);
        }

        // TODO: Koen
        //[Fact]
        //public void UseInitialForAssigned()
        //{
        //    var before = new Datas(this.Transaction).Extent().ToArray();

        //    this.page.AutocompleteDerivedFilter.Select("jane@example.com");

        //    this.page.SAVE.Click();

        //    this.Driver.WaitForAngular();
        //    this.Transaction.Rollback();

        //    var after = new Datas(this.Transaction).Extent().ToArray();

        //    Assert.Equal(after.Length, before.Length + 1);

        //    var data = after.Except(before).First();

        //    Assert.Null(data.AutocompleteAssignedFilter);
        //    Assert.Null(data.AutocompleteDerivedFilter);
        //}

        [Fact]
        public void UseOtherForAssigned()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            this.page.AutocompleteDerivedFilter.Select("jenny@example.com");

            this.page.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var data = after.Except(before).First();

            Assert.Equal(this.jenny, data.AutocompleteAssignedFilter);
            Assert.Equal(this.jenny, data.AutocompleteDerivedFilter);
        }
    }
}