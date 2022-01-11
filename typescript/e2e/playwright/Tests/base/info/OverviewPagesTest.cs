// <copyright file="OverviewPagesTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.ApplicationTests
{
    using System.Linq;
    using Allors.Database;
    using Allors.Database.Domain;
    using Allors.E2E.Angular;
    using Allors.E2E.Angular.Info;
    using Allors.E2E.Angular.Material.Factory;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class OverviewPagesTest : Test
    {
        public ApplicationInfo Application => this.AppRoot.ApplicationInfo;

        public ComponentInfo[] Components => this.Application.ComponentInfoByName.Values.ToArray();

        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task Open()
        {
            foreach (var component in this.Components.Where(v => v.Overview != null))
            {
                var objectType = component.Overview;
                foreach (IObject @object in this.Transaction.Extent(objectType))
                {
                    var url = component.RouteInfo.FullPath.Replace(":id", $"{@object.Strategy.ObjectId}");
                    await this.Page.GotoAsync(url);
                    await this.Page.WaitForAngular();
                }
            }

            Assert.IsEmpty(this.ConsoleErrorMessages);
        }

        [Test]
        public async Task Detail()
        {
            foreach (var component in this.Components.Where(v => v.Overview != null))
            {
                var objectType = component.Overview;

                if (!objectType.Equals(M.Organisation))
                {
                    continue;
                }

                foreach (IObject @object in this.Transaction.Extent(objectType))
                {
                    var url = component.RouteInfo.FullPath.Replace(":id", $"{@object.Strategy.ObjectId}");
                    await this.Page.GotoAsync(url);
                    await this.Page.WaitForAngular();

                    var detail = this.AppRoot.Locator.Locator("[data-allors-kind='panel-detail']");
                    await detail.ClickAsync();
                    await this.Page.WaitForAngular();

                    var cancel = this.AppRoot.Locator.Locator("[data-allors-kind='cancel']");
                    await cancel.ClickAsync();
                    await this.Page.WaitForAngular();
                }
            }

            Assert.IsEmpty(this.ConsoleErrorMessages);
        }

        [Test]
        public async Task ObjectRelation()
        {
            foreach (var component in this.Components.Where(v => v.Overview != null))
            {
                var objectType = component.Overview;

                if (!objectType.Equals(M.Organisation))
                {
                    continue;
                }

                foreach (IObject @object in this.Transaction.Extent(objectType))
                {
                    var url = component.RouteInfo.FullPath.Replace(":id", $"{@object.Strategy.ObjectId}");
                    await this.Page.GotoAsync(url);
                    await this.Page.WaitForAngular();

                    var objectRelations = this.AppRoot.Locator.Locator("[data-allors-kind='panel-relation-object']");

                    for (var i = 0; i < await objectRelations.CountAsync(); i++)
                    {
                        var objectRelation = objectRelations.Nth(i);

                        await objectRelation.ClickAsync();
                        await this.Page.WaitForAngular();

                        var fab = new FactoryFabComponent(this.AppRoot);

                        foreach (var @class in objectType.Classes)
                        {
                            await fab.Create(@class);

                            var cancel = this.OverlayContainer.Locator.Locator("[data-allors-kind='cancel']");
                            await cancel.ClickAsync();
                            await this.Page.WaitForAngular();
                        }

                        var close = this.AppRoot.Locator.Locator("mat-icon:has-text('expand_less')");
                        await close.ClickAsync();
                        await this.Page.WaitForAngular();
                    }
                }
            }

            Assert.IsEmpty(this.ConsoleErrorMessages);
        }

        //[Test]
        //public async void PanelsCreate()
        //{
        //    foreach (var page in this.OverviewPages())
        //    {
        //        var panelProperties = page.GetType().GetProperties().Where(v => v.Name.ToUpperInvariant().Contains("PANEL"));
        //        foreach (var panelProperty in panelProperties)
        //        {
        //            var panel = (SelectorComponent)panelProperty.GetGetMethod().Invoke(page, null);
        //            if (this.Driver.SelectorIsVisible(panel.Selector))
        //            {
        //                dynamic dynamicPanel = panel;
        //                dynamicPanel.Click();

        //                if (panel.GetType().GetProperties().Any(v => v.Name.Equals("Factory")))
        //                {
        //                    var factory = (MatFactoryFab)dynamicPanel.Factory;

        //                    if (this.Driver.SelectorIsVisible(factory.Selector))
        //                    {
        //                        var classes = factory.Classes;

        //                        foreach (var @class in classes)
        //                        {
        //                            factory.Create(@class);
        //                            var dialog = this.Driver.GetDialog(this.M);
        //                            Cancel(dialog);
        //                        }
        //                    }
        //                }

        //                this.Collapse(panel);
        //            }
        //        }
        //    }
        //}

        //[Test]
        //public async void PanelsEdit()
        //{
        //    foreach (var page in this.OverviewPages())
        //    {
        //        var panelProperties = page.GetType().GetProperties().Where(v => v.Name.ToUpperInvariant().Contains("PANEL"));
        //        foreach (var panelProperty in panelProperties)
        //        {
        //            var panel = (SelectorComponent)panelProperty.GetGetMethod().Invoke(page, null);
        //            if (this.Driver.SelectorIsVisible(panel.Selector))
        //            {
        //                dynamic dynamicPanel = panel;
        //                dynamicPanel.Click();

        //                var tableProperty = panel.GetType().GetProperties().FirstOrDefault(v => v.PropertyType == typeof(MatTable));
        //                if (tableProperty != null)
        //                {
        //                    var table = (MatTable)tableProperty?.GetGetMethod().Invoke(panel, null);
        //                    var action = table.Actions.FirstOrDefault(v => v.Equals("edit"));

        //                    if (action != null)
        //                    {
        //                        var objects = this.Transaction.Instantiate(table.ObjectIds);
        //                        foreach (var @object in objects)
        //                        {
        //                            table.Action(@object, action);

        //                            this.Driver.WaitForAngular();
        //                            var dialogElement = this.Driver.FindElement(By.CssSelector("mat-dialog-container ng-component[data-test-scope]"));
        //                            var testScope = dialogElement.GetAttribute("data-test-scope");
        //                            var type = Assembly.GetExecutingAssembly().GetTypes().First(v => v.Name.Equals(testScope));
        //                            var dialog = (Component)Activator.CreateInstance(type, this.Driver, this.M);

        //                            Cancel(dialog);
        //                        }
        //                    }
        //                }

        //                this.Collapse(panel);
        //            }
        //        }
        //    }
        //}

        //private IEnumerable<Component> OverviewPages()
        //{
        //    this.Login();

        //    foreach (var navigateTo in this.navigateTos)
        //    {
        //        var listPage = (Component)navigateTo.Invoke(this.Sidenav, null);

        //        var tableProperty = listPage.GetType().GetProperties().FirstOrDefault(v => v.PropertyType == typeof(MatTable));
        //        if (tableProperty != null)
        //        {
        //            var table = (MatTable)tableProperty?.GetGetMethod().Invoke(listPage, null);
        //            var action = table.Actions.FirstOrDefault(v => v.Equals("overview"));

        //            if (action != null)
        //            {
        //                var objects = this.Transaction.Instantiate(table.ObjectIds);
        //                foreach (var @object in objects)
        //                {
        //                    listPage = (Component)navigateTo.Invoke(this.Sidenav, null);
        //                    table.Action(@object, action);

        //                    this.Driver.WaitForAngular();
        //                    var dialogElement = this.Driver.FindElement(By.CssSelector("mat-sidenav-content ng-component[data-test-scope]"));
        //                    var testScope = dialogElement.GetAttribute("data-test-scope");
        //                    var type = Assembly.GetExecutingAssembly().GetTypes().First(v => v.Name.Equals(testScope));
        //                    var page = (Component)Activator.CreateInstance(type, this.Driver, this.M);

        //                    yield return page;
        //                }
        //            }
        //        }
        //    }
        //}

        //private void Collapse(SelectorComponent panel)
        //{
        //    this.Driver.WaitForAngular();

        //    var byChained = new ByChained(panel.Selector, By.XPath($".//mat-icon[contains(text(), 'expand_less')]"));
        //    var collapse = this.Driver.FindElement(byChained);
        //    collapse.Click();
        //}

        //private static void Cancel(Component dialog)
        //{
        //    var cancelProperty = dialog?.GetType().GetProperties().FirstOrDefault(v => v.Name.Equals("cancel", StringComparison.InvariantCultureIgnoreCase));
        //    dynamic cancel = cancelProperty?.GetGetMethod().Invoke(dialog, null);

        //    cancel?.Click();
        //}
    }
}
