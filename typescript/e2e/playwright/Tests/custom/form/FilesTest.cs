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

    public class FilesTest : Test
    {
        public FormComponent FormComponent => new FormComponent(this.AppRoot);

        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
            await this.GotoAsync("/form");
        }

        [Test]
        public async Task UploadOne()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            var file = new FileInfo("logo.png");
            await this.FormComponent.MultipleFiles.UploadAsync(file);

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.AreEqual(1, data.MultipleFiles.Count());
        }

        [Test]
        public async Task UploadTwo()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            var file1 = new FileInfo("logo.png");
            await this.FormComponent.MultipleFiles.UploadAsync(file1);

            var file2 = new FileInfo("logo2.png");
            await this.FormComponent.MultipleFiles.UploadAsync(file2);

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();

            var after = new Datas(this.Transaction).Extent().ToArray();
            Assert.AreEqual(after.Length, before.Length + 1);
            var data = after.Except(before).First();
            Assert.AreEqual(2, data.MultipleFiles.Count());
        }

        [Test]
        public async Task Remove()
        {
            var before = new Datas(this.Transaction).Extent().ToArray();

            var file1 = new FileInfo("logo.png");
            await this.FormComponent.MultipleFiles.UploadAsync(file1);

            var file2 = new FileInfo("logo2.png");
            await this.FormComponent.MultipleFiles.UploadAsync(file2);

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();
            var after = new Datas(this.Transaction).Extent().ToArray();
            var data = after.Except(before).First();

            var logo1 = data.MultipleFiles.First(v => v.Name.Equals("logo"));
            var logo2 = data.MultipleFiles.First(v => v.Name.Equals("logo2"));

            var media = this.FormComponent.MultipleFiles.Media(logo1);
            await media.RemoveAsync();

            await this.FormComponent.SaveAsync();
            this.Transaction.Rollback();

            Assert.AreEqual(1, data.MultipleFiles.Count());
            Assert.AreEqual(logo2, data.MultipleFiles.First());
        }
    }
}
