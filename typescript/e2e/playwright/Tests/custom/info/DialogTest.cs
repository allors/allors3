// <copyright file="InputTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Info
{
    using Allors.E2E.Angular.Info;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class DialogTest : Test
    {
        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
            await this.GotoAsync("/form");
        }

        [Test]
        public async Task Create()
        {
            var dialogInfo = await this.AppRoot.GetDialogsInfo();

            var create = dialogInfo.Create;
            Assert.NotNull(create);
            Assert.IsNotEmpty(create);
        }

        [Test]
        public async Task Edit()
        {
            var dialogInfo = await this.AppRoot.GetDialogsInfo();

            var edit = dialogInfo.Edit;
            Assert.NotNull(edit);
            Assert.IsNotEmpty(edit);
        }
    }
}
