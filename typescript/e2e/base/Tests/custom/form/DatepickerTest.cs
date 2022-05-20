// <copyright file="AutoCompleteFilterTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.E2E.Form
{
    using System.Globalization;
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.E2E.Angular;
    using Allors.E2E.Test;
    using E2E;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class DatepickerTest : Test
    {
        public FieldsFormComponent FormComponent => new FieldsFormComponent(this.AppRoot);

        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
            await this.Page.NavigateAsync("/fields");
        }

        [Test]
        public async Task Initial()
        {
            CultureInfo.CurrentCulture = new CultureInfo("nl-BE");

            var date = await this.FormComponent.DateDatepicker.GetAsync();

            Assert.IsNull(date);
        }

        [Test]
        public async Task SetDate()
        {
            CultureInfo.CurrentCulture = new CultureInfo("nl-BE");

            var before = new Datas(this.Transaction).Extent().ToArray();

            var now = this.Transaction.Now();

            await this.FormComponent.DateDatepicker.SetAsync(now);

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(before.Length + 1, after.Length);
            var data = after.Except(before).First();
            Assert.True(data.Date != null);
            Assert.AreEqual(now.Year, data.Date.Value.Year);
            Assert.AreEqual(now.Month, data.Date.Value.Month);
            Assert.AreEqual(now.Day, data.Date.Value.Day);
        }
    }
}
