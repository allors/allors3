// <copyright file="ListPagesTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.ApplicationTests
{
    using System.Linq;
    using Allors.E2E.Angular;
    using Allors.E2E.Angular.Info;
    using Allors.E2E.Angular.Material.Factory;
    using Allors.E2E.Angular.Material.Table;
    using Microsoft.Playwright;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class ListPagesTest : Test
    {
        public ApplicationInfo Application => this.AppRoot.ApplicationInfo;

        public ComponentInfo[] Components => this.Application.ComponentInfoByName.Values.ToArray();

        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task Open()
        {
            foreach (var component in this.Components.Where(v => v.List != null))
            {
                await this.Page.GotoAsync(component.RouteInfo.FullPath);
                await this.Page.WaitForAngular();
            }
        }

        [Test]
        public async Task Create()
        {
            foreach (var component in this.Components.Where(v => v.List != null))
            {
                await this.Page.GotoAsync(component.RouteInfo.FullPath);
                await this.Page.WaitForAngular();

                var factory = new FactoryFabComponent(this.AppRoot);

                var count = await factory.Locator.CountAsync();
                if (count == 0)
                {
                    continue;
                }

                var objectType = await factory.ObjectType();

                foreach (var @class in objectType.Classes.Where(v => v.WorkspaceNames.Contains(this.WorkspaceName)))
                {
                    await factory.Create(@class);
                    await this.Page.WaitForAngular();

                    await this.Page.Keyboard.PressAsync("Escape");
                    await this.Page.WaitForAngular();
                }
            }
        }

        [Test]
        public async Task Edit()
        {
            foreach (var component in this.Components.Where(v => v.List != null))
            {
                await this.Page.GotoAsync(component.RouteInfo.FullPath);
                await this.Page.WaitForAngular();

                var table = new AllorsMaterialTableComponent(this.AppRoot);
                var actions = await table.Actions();
                if (actions.Contains("edit"))
                {
                    foreach (var @object in this.Transaction.Instantiate(await table.GetObjectIds()))
                    {
                        await this.Page.WaitForAngular();
                        await table.Action(@object, "edit");

                        await this.Page.WaitForAngular();
                        await this.Page.Keyboard.PressAsync("Escape");
                    }
                }
            }
        }

        [Test]
        public async Task Overview()
        {
            foreach (var component in this.Components.Where(v => v.List != null))
            {
                await this.Page.GotoAsync(component.RouteInfo.FullPath);
                await this.Page.WaitForAngular();

                var table = new AllorsMaterialTableComponent(this.AppRoot);
                var actions = await table.Actions();
                if (actions.Contains("overview"))
                {
                    foreach (var @object in this.Transaction.Instantiate(await table.GetObjectIds()))
                    {
                        await this.Page.WaitForAngular();
                        await table.Action(@object, "overview");

                        await this.Page.WaitForAngular();
                        await this.Page.GoBackAsync();
                    }
                }
            }
        }
    }
}
