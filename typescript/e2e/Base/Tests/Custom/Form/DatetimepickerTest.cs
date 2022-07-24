// <copyright file="AutoCompleteFilterTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.E2E.Form
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.E2E.Angular;
    using Allors.E2E.Test;
    using E2E;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class DatetimepickerTest : Test
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

            var date = await this.FormComponent.DateTimeDatetimepicker.GetAsync();

            Assert.IsNull(date);
        }

        [Test]
        public async Task Populated()
        {
            CultureInfo.CurrentCulture = new CultureInfo("nl-BE");

            var data = new DataBuilder(this.Transaction).Build();
            data.DateTime = this.Transaction.Database.Services.Get<ITime>().Now();
            this.Transaction.Commit();

            await this.Page.NavigateAsync("/");
            await this.Page.NavigateAsync("/fields");

            var actual = await this.FormComponent.DateTimeDatetimepicker.GetAsync();

            Assert.That(actual, Is.EqualTo(data.DateTime.Value.ToLocalTime()).Within(1).Minutes);
        }

        [Test]
        public async Task SetDate()
        {
            CultureInfo.CurrentCulture = new CultureInfo("nl-BE");

            var before = new Datas(this.Transaction).Extent().ToArray();

            var date = new DateTime(2018, 1, 1, 12, 0, 0, DateTimeKind.Local);

            await this.FormComponent.DateTimeDatetimepicker.SetAsync(date);

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.True(data.ExistDateTime);
            Assert.AreEqual(date, data.DateTime.Value.ToLocalTime());
        }

        [Test]
        public async Task ChangeDate()
        {
            CultureInfo.CurrentCulture = new CultureInfo("nl-BE");

            var before = new Datas(this.Transaction).Extent().ToArray();

            var date = new DateTime(2019, 1, 1, 12, 0, 0, DateTimeKind.Local);
            await this.FormComponent.DateTimeDatetimepicker.SetAsync(date);

            await this.FormComponent.SaveAsync();

            date = new DateTime(2019, 1, 1, 18, 0, 0, DateTimeKind.Local);
            await this.FormComponent.DateTimeDatetimepicker.SetAsync(date);

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.AreEqual(date, data.DateTime.Value.ToLocalTime());
        }
    }
}
