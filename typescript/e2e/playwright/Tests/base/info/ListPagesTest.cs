// <copyright file="ListPagesTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.ApplicationTests
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using Allors.E2E.Angular;
    using Allors.E2E.Angular.Info;
    using Allors.E2E.Angular.Material.Factory;
    using Allors.E2E.Angular.Material.Table;
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

            Assert.IsEmpty(this.ConsoleErrorMessages);
        }

        [Test]
        public async Task Create()
        {
            foreach (var component in this.Components.Where(v => v.List != null))
            {
                await this.Page.GotoAsync(component.RouteInfo.FullPath);
                await this.Page.WaitForAngular();

                var factory = new FactoryFabComponent(this.AppRoot);
                var objectType = await factory.ObjectType();

                foreach (var @class in objectType.Classes.Where(v => v.WorkspaceNames.Contains(this.WorkspaceName)))
                {
                    await factory.Create(@class);
                    await this.Page.WaitForAngular();
                }
            }

            Assert.IsEmpty(this.ConsoleErrorMessages);
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
                        await table.Action(@object, "edit");
                        await this.Page.WaitForAngular();


                    }
                }
            }

            Assert.IsEmpty(this.ConsoleErrorMessages);
        }

        //[Test]
        //public async Task Overview()
        //{
        //    this.Login();

        //    foreach (var navigateTo in this.navigateTos)
        //    {
        //        var page = (Component)navigateTo.Invoke(this.Sidenav, null);

        //        var tableProperty = page.GetType().GetProperties().FirstOrDefault(v => v.PropertyType == typeof(MatTable));
        //        if (tableProperty != null)
        //        {
        //            var table = (MatTable)tableProperty?.GetGetMethod().Invoke(page, null);

        //            if (this.Driver.SelectorIsVisible(table.Selector))
        //            {
        //                var action = table.Actions.FirstOrDefault(v => v.Equals("overview"));
        //                if (action != null)
        //                {
        //                    var objects = this.Transaction.Instantiate(table.ObjectIds);
        //                    foreach (var @object in objects)
        //                    {
        //                        table.Action(@object, action);
        //                        this.Driver.Navigate().Back();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //private static void Cancel(IComponent dialog)
        //{
        //    var cancelProperty = dialog?.GetType().GetProperties().FirstOrDefault(v => v.Name.Equals("cancel", StringComparison.InvariantCultureIgnoreCase));
        //    dynamic cancel = cancelProperty?.GetGetMethod().Invoke(dialog, null);

        //    cancel?.Click();
        //}
    }
}
