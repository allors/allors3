// <copyright file="ChipsTest.cs" company="Allors bvba">
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
    public class ChipsTest : Test
    {
        private readonly FormComponent page;

        public ChipsTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.page = this.Sidenav.NavigateToForm();
        }

        [Fact]
        public void AddOne()
        {
            var jane = new People(this.Transaction).FindBy(this.M.Person.UserEmail, "jane@example.com");
            var before = new Datas(this.Transaction).Extent().ToArray();

            this.page.Chips.Add("jane", "jane@example.com");

            this.page.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var data = after.Except(before).First();

            Assert.Contains(jane, data.Chips);
        }

        [Fact]
        public void AddTwo()
        {
            var jane = new People(this.Transaction).FindBy(this.M.Person.UserEmail, "jane@example.com");
            var john = new People(this.Transaction).FindBy(this.M.Person.UserEmail, "john@example.com");
            var before = new Datas(this.Transaction).Extent().ToArray();

            this.page.Chips.Add("jane", "jane@example.com");

            this.page.Chips.Add("john", "john@example.com");

            this.page.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var data = after.Except(before).First();

            Assert.Contains(jane, data.Chips);
            Assert.Contains(john, data.Chips);
        }

        [Fact]
        public void RemoveOne()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            this.page.Chips.Add("jane", "jane@example.com");

            this.page.SAVE.Click();

            this.page.Chips.Remove("jane@example.com");

            this.page.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var data = after.Except(before).First();

            Assert.Empty(data.Chips);
        }
    }
}
