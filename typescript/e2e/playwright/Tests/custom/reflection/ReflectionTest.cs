// <copyright file="InputTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Form
{
    using Autotest;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class ReflectionTest : Test
    {
        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
            await this.GotoAsync("/tests/form");
        }

        [Test]
        public async Task Meta()
        {
            var model = new Reflection(this.AppRoot, this.M);

            var menu = await model.Menu();
            var metaExtensions = await model.MetaExtensions();
        }
    }
}
