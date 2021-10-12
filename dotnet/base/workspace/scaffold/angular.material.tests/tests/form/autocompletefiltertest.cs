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
    public class AutoCompleteFilterTest : Test
    {
        private readonly FormComponent page;

        public AutoCompleteFilterTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.page = this.Sidenav.NavigateToForm();
        }

        [Fact]
        public void Full()
        {
            var administrator = new People(this.Transaction).FindBy(this.M.Person.UserEmail, "administrator");
            var before = new Datas(this.Transaction).Extent().ToArray();

            this.page.AutocompleteFilter.Select("administrator");

            this.page.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var data = after.Except(before).First();

            Assert.Equal(administrator, data.AutocompleteFilter);
        }


        [Fact]
        public void PartialWithSelection()
        {
            var administrator = new People(this.Transaction).FindBy(this.M.Person.UserEmail, "administrator");
            var before = new Datas(this.Transaction).Extent().ToArray();

            this.page.AutocompleteFilter.Select("administrator", "administrator");

            this.page.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var data = after.Except(before).First();

            Assert.Equal(administrator, data.AutocompleteFilter);
        }
    }
}
