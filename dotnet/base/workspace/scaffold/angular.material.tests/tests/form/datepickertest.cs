// <copyright file="DatepickerTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Globalization;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    public class DatepickerTest : Test
    {
        private readonly FormComponent page;

        public DatepickerTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.page = this.Sidenav.NavigateToForm();
        }

        [Fact]
        public void Initial()
        {
            CultureInfo.CurrentCulture = new CultureInfo("nl-BE");

            var before = new Datas(this.Transaction).Extent().ToArray();

            var date = this.Transaction.Now();
            this.page.Date.Value = date;

            this.page.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();

            Assert.Equal(before.Length + 1, after.Length);

            var data = after.Except(before).First();

            Assert.True(data.ExistDate);
        }
    }
}
