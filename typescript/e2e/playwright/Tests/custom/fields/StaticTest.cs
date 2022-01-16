// <copyright file="AutoCompleteFilterTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Form
{
    using Allors.Database.Domain;
    using Allors.E2E.Angular.Material.Form;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class StaticTest : Test
    {
        public FormComponent FormComponent => new FormComponent(this.AppRoot);

        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
            await this.GotoAsync("/fields");
        }

        [Test]
        public async Task Populated()
        {
            var data = new DataBuilder(this.Transaction).Build();
            data.Static = "A Static String!";
            this.Transaction.Commit();

            await this.GotoAsync("/fields");

            var actual = await this.FormComponent.Static.GetAsync();

            Assert.That(actual, Is.EqualTo("A Static String!"));
        }
    }
}
