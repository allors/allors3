// <copyright file="SelectTest.cs" company="Allors bvba">
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
    public class SelectDerivedTest : Test
    {
        private readonly FormComponent page;

        private readonly Person john;
        private readonly Person administrator;
        private readonly Person jenny;

        public SelectDerivedTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.page = this.Sidenav.NavigateToForm();

            this.john = new People(this.Transaction).FindBy(this.M.Person.UserEmail, "john@example.com");
            this.administrator = new People(this.Transaction).FindBy(this.M.Person.UserEmail, "administrator");
            this.jenny = new People(this.Transaction).FindBy(this.M.Person.UserEmail, "jenny@example.com");

            var singleton = this.Transaction.GetSingleton();
            // TODO: Koen
            //singleton.SelectDefault = john;

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

            Assert.Null(data.SelectAssigned);
            Assert.Equal(this.john, data.SelectDerived);
        }

        [Fact]
        public void UseInitialForAssigned()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            this.page.SelectDerived.Select(this.administrator);

            this.page.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var data = after.Except(before).First();

            Assert.Null(data.SelectAssigned);
            Assert.Equal(this.john, data.SelectDerived);
        }

        [Fact]
        public void UseOtherForAssigned()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            this.page.SelectDerived.Select(this.jenny);

            this.page.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var data = after.Except(before).First();

            Assert.Equal(this.jenny, data.SelectAssigned);
            Assert.Equal(this.jenny, data.SelectDerived);
        }
    }
}
