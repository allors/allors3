// <copyright file="InputTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Microsoft.Playwright;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class InputTest : Test
    {
        public override void Configure(BrowserTypeLaunchOptions options) => options.Headless = false;

        [SetUp]
        public async Task Setup()
        {
            this.
        }

        [Test]
        public void String()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            this.page.String.Value = "Hello";

            this.page.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var data = after.Except(before).First();

            Assert.Equal("Hello", data.String);
        }
    }
}
