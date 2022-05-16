// <copyright file="ListPagesTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.E2E.Generic
{
    using System.Linq;
    using Allors.E2E.Angular.Info;
    using Allors.E2E.Angular.Material.Sidenav;
    using NUnit.Framework;
    using Tests;
    using Task = System.Threading.Tasks.Task;

    public class MenuTest : Test
    {
        public Sidenav Sidenav => new Sidenav(this.AppRoot);

        public ApplicationInfo Application => this.AppRoot.ApplicationInfo;

        [Test]
        public async Task Navigate()
        {
            foreach (var user in this.Fixture.Logins)
            {
                await this.LoginAsync(user);

                var components = this.Application.ComponentInfoByName.Values.Where(v => v.MenuInfo != null).ToArray();

                Assert.IsNotEmpty(components);

                foreach (var component in components)
                {
                    await this.Sidenav.NavigateAsync(component);

                }
            }
        }
    }
}
