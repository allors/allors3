// <copyright file="OverviewPagesTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.E2E.Generic
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


        [Test]
        public async Task Open()
        {
            foreach (var user in this.Fixture.Logins)
            {
                await this.LoginAsync(user);

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
            }
        }

        [Test]
        public async Task Detail()
        {
            foreach (var user in this.Fixture.Logins)
            {
                await this.LoginAsync(user);


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

                        var detail = this.AppRoot.Locator.Locator("[data-allors-kind='view-detail-panel']");
                        await detail.ClickAsync();
                        await this.Page.WaitForAngular();

                        var cancel = this.AppRoot.Locator.Locator("[data-allors-kind='cancel']");
                        await cancel.ClickAsync();
                        await this.Page.WaitForAngular();
                    }
                }
            }
        }

        [Test]
        public async Task ObjectRelation()
        {
            foreach (var user in this.Fixture.Logins)
            {
                await this.LoginAsync(user);

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

                        var objectRelations =
                            this.AppRoot.Locator.Locator("[data-allors-kind='panel-relation-object']");

                        for (var i = 0; i < await objectRelations.CountAsync(); i++)
                        {
                            var objectRelation = objectRelations.Nth(i);

                            await objectRelation.ClickAsync();
                            await this.Page.WaitForAngular();

                            // Create
                            var fab = new FactoryFabComponent(this.AppRoot);
                            foreach (var @class in objectType.Classes)
                            {
                                await fab.Create(@class);

                                var cancel = this.OverlayContainer.Locator.Locator("[data-allors-kind='cancel']");
                                await cancel.ClickAsync();
                                await this.Page.WaitForAngular();
                            }

                            // Edit
                            var rows = objectRelation.Locator("tr[data-allors-id]");
                            for (var j = 0; j < await rows.CountAsync(); j++)
                            {
                                var row = rows.Nth(i);

                                await row.ClickAsync();

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
            }
        }
    }
}
