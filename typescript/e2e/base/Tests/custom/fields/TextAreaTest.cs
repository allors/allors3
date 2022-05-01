// <copyright file="InputTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Form
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.E2E.Test;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class TextAreaTest : Test
    {
        public FieldsFormComponent FormComponent => new FieldsFormComponent(this.AppRoot);

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
            data.PlainText = "This is plain text.";
            this.Transaction.Commit();

            await this.GotoAsync("/fields");

            var actual = await this.FormComponent.PlainText.GetAsync();

            Assert.That(actual, Is.EqualTo("This is plain text."));
        }

        [Test]
        public async Task Set()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            await this.FormComponent.PlainText.SetAsync("Hello");

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.AreEqual("Hello", data.PlainText);
        }
    }
}
