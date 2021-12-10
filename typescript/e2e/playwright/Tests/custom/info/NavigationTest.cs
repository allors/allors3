// <copyright file="InputTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Info
{
    using System.Linq;
    using Autotest;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class NavigationTest : Test
    {
        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
            await this.GotoAsync("/tests/form");
        }

        [Test]
        public async Task Navigation()
        {
            var navigationInfo = await this.AppRoot.GetNavigationInfo();

            var list = navigationInfo.Where(v => v.List != null).ToArray();
            var overview = navigationInfo.Where(v => v.List != null).ToArray();

            Assert.NotNull(navigationInfo);
            Assert.IsNotEmpty(navigationInfo);
        }
    }
}
