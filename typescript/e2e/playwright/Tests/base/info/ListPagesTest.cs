// <copyright file="ListPagesTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.ApplicationTests
{
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class ListPagesTest : Test
    {
        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
        }

        //[Test]
        //public async Task Create()
        //{
        //    foreach (var navigateTo in this.navigateTos)
        //    {
        //        var page = (IComponent)navigateTo.Invoke(this.Sidenav, null);
        //        if (page.GetType().GetProperties().Any(v => v.Name.Equals("Factory")))
        //        {
        //            var factory = (FactoryFabComponent)((dynamic)page).Factory;

        //            if (await factory.Locator.IsVisibleAsync())
        //            {
        //                var classes = await factory.Classes();

        //                foreach (var @class in classes)
        //                {
        //                    await factory.Create(@class);

        //                    var dialog = new .GetDialog(this.M);
        //                    Cancel(dialog);
        //                }
        //            }
        //        }
        //    }
        //}

        //[Test]
        //public async Task Edit()
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
        //                var action = table.Actions.FirstOrDefault(v => v.Equals("edit"));

        //                if (action != null)
        //                {
        //                    var objects = this.Transaction.Instantiate(table.ObjectIds);
        //                    foreach (var @object in objects)
        //                    {
        //                        table.Action(@object, action);
        //                        var dialog = this.Driver.GetDialog(this.M);
        //                        Cancel(dialog);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

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
