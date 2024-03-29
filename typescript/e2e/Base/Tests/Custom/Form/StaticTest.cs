// <copyright file="AutoCompleteFilterTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.E2E.Form
{
    using Allors.Database.Domain;
    using Allors.E2E.Angular;
    using Allors.E2E.Test;
    using E2E;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class StaticTest : Test
    {
        public FieldsFormComponent FormComponent => new FieldsFormComponent(this.AppRoot);

        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
            await this.Page.NavigateAsync("/fields");
        }

        [Test]
        public async Task Populated()
        {
            var data = new DataBuilder(this.Transaction).Build();
            data.Static = "A Static String!";
            this.Transaction.Commit();

            await this.Page.NavigateAsync("/");
            await this.Page.NavigateAsync("/fields");

            var actual = await this.FormComponent.StaticStatic.GetAsync();

            Assert.That(actual, Is.EqualTo("A Static String!"));
        }
    }
}
