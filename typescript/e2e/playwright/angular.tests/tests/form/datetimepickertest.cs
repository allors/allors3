// <copyright file="DatetimepickerTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Linq;
    using Allors.Database.Domain;
    using Components;
    using src.app.tests.form;
    using Xunit;

    [Collection("Test collection")]
    public class DatetimepickerTest : Test
    {
        private FormComponent page;

        public DatetimepickerTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.page = this.Sidenav.NavigateToForm();
        }

        [Fact]
        public void Populated()
        {
            var data = new DataBuilder(this.Transaction).Build();

            var expected = this.Transaction.Database.Services.Get<ITime>().Now();

            data.DateTime = expected.ToUniversalTime();
            this.Transaction.Commit();

            this.Sidenav.NavigateToHome();
            this.page = this.Sidenav.NavigateToForm();

            var actual = this.page.DateTime.Value.Value;
            Assert.Equal(expected.Date, actual.Date);
        }

        [Fact]
        public void Initial()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            var date = new DateTime(2018, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            this.page.DateTime.Value = date;

            this.page.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var data = after.Except(before).First();

            Assert.True(data.ExistDateTime);
            Assert.Equal(date, data.DateTime);
        }

        [Fact]
        public void Change()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            var date = new DateTime(2019, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            this.page.DateTime.Value = date;

            this.page.SAVE.Click();

            date = new DateTime(2019, 1, 1, 18, 0, 0, DateTimeKind.Utc);
            this.page.DateTime.Value = date;

            this.page.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var data = after.Except(before).First();

            Assert.Equal(date, data.DateTime);
        }
    }
}
