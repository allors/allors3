// <copyright file="PersonListTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Relations
{
    using Allors.Database.Domain;
    using Microsoft.Playwright;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class PersonListTest : Test
    {
        public override void Configure(BrowserTypeLaunchOptions options) => options.Headless = false;

        public override void Configure(BrowserNewContextOptions options) => options.BaseURL = "http://localhost:4200";

        public PeopleComponent PeopleComponent => new PeopleComponent(this.AppRoot);

        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
            await this.GotoAsync("/contacts/people");
        }

        [Test]
        public void Table()
        {
            var person = new People(this.Transaction).FindBy(M.Person.FirstName, "John");
            var row = this.PeopleComponent.Table.FindRow(person);
            var cell = row.FindCell("firstName");
            cell.Click();
        }
    }
}
