// <copyright file="InputTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.E2E.Info
{
    using Allors.E2E.Angular.Info;
    using E2E;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class MenuTest : Test
    {
        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
            await this.GotoAsync("/fields");
        }

        [Test]
        public async Task Menu()
        {
            var menuInfo = await this.AppRoot.GetMenuInfos();

            Assert.NotNull(menuInfo);
            Assert.IsNotEmpty(menuInfo);
        }
    }
}
