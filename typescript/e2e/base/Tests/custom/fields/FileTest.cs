// <copyright file="AutoCompleteFilterTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Form
{
    using System.IO;
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.E2E.Angular.Material.Form;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class FileTest : Test
    {
        public FormComponent FormComponent => new FormComponent(this.AppRoot);

        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
            await this.GotoAsync("/fields");
        }

        [Test]
        public async Task Upload()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            var file = new FileInfo("logo.png");
            await this.FormComponent.File.UploadAsync(file);

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.True(data.ExistFile);
        }

        [Test]
        public async Task Remove()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            var file = new FileInfo("logo.png");
            await this.FormComponent.File.UploadAsync(file);

            await this.FormComponent.SaveAsync();

            this.Transaction.Rollback();
            var after = new Datas(this.Transaction).Extent().ToArray();
            var data = after.Except(before).First();

            var media = this.FormComponent.File.Media(data.File);
            await media.RemoveAsync();

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();

            Assert.False(data.ExistFile);
        }
    }
}
