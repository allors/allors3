// <copyright file="InputTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Info
{
    using Allors.E2E.Angular.Info;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class ApplicationInfoTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task New()
        {
            var application = await ApplicationInfo.New(this.AppRoot);

            Assert.IsNotEmpty(application.CreateComponentByObjectType);
            Assert.IsNotEmpty(application.EditComponentByObjectType);
        }
    }
}
