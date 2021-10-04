// <copyright file="AutoCompleteFilterTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using Components;
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
            var jane = new Users(this.Transaction).GetUser("jane@example.com");
            var before = new Datas(this.Transaction).Extent().ToArray();

            this.page.AutocompleteFilter.Select("jane@example.com");

            this.page.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var data = after.Except(before).First();

            Assert.Equal(jane, data.AutocompleteFilter);
        }


        [Fact]
        public void PartialWithSelection()
        {
            var jane = new Users(this.Transaction).GetUser("jane@example.com");
            var before = new Datas(this.Transaction).Extent().ToArray();

            this.page.AutocompleteFilter.Select("jane", "jane@example.com");

            this.page.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var data = after.Except(before).First();

            Assert.Equal(jane, data.AutocompleteFilter);
        }
    }
}
