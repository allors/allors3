// <copyright file="InputTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Info
{
    using Allors.E2E.Angular.Info;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class RouteTest : Test
    {
        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
            await this.GotoAsync("/fields");
        }

        [Test]
        public async Task Route()
        {
            var routeInfos = await this.AppRoot.GetRouteInfos();

            Assert.NotNull(routeInfos);
            Assert.IsNotEmpty(routeInfos);
        }
    }
}
