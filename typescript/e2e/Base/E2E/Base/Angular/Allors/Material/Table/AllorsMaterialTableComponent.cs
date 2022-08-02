// <copyright file="MatList.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Table
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Allors.Database;
    using Allors.Database.Meta;
    using Info;
    using Menu;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    public class AllorsMaterialTableComponent : IComponent
    {
        public AllorsMaterialTableComponent(IComponent container)
        {
            this.Container = container;
            this.Locator = this.Container.Locator.Locator("a-mat-table");
        }

        public IComponent Container { get; }

        public MetaPopulation M => this.Container.M;

        public IPage Page => this.Container.Page;

        public ILocator Locator { get; }

        public ApplicationInfo ApplicationInfo => this.Container.ApplicationInfo;

        public async Task<string[]> GetObjectIds()
        {
            await this.Page.WaitForAngular();

            var rows = this.Locator.Locator("tr[mat-row][data-allors-id]");

            var objectIds = new List<string>();
            for (var i = 0; i < await rows.CountAsync(); i++)
            {
                var row = rows.Nth(i);
                var objectId = await row.GetAttributeAsync("data-allors-id");
                objectIds.Add(objectId);
            }

            return objectIds.ToArray();
        }

        public async Task<string[]> Actions()
        {
            await this.Page.WaitForAngular();

            var table = this.Locator.Locator("table");
            var attribute = await table.GetAttributeAsync("data-allors-actions");
            return !string.IsNullOrWhiteSpace(attribute) ? attribute.Split(",") : Array.Empty<string>();
        }

        public MatTableRow FindRow(IObject obj)
        {
            var row = this.TableRowElement(obj);
            return new MatTableRow(this.Page, row);
        }

        public async Task DefaultAction(IObject obj)
        {
            var row = this.FindRow(obj);
            var cell = (await row.GetCells())[1];
            await cell.Click();
        }

        public async Task Action(IObject obj, string action)
        {
            var row = this.FindRow(obj);
            var cell = row.FindCell("menu");
            await cell.Click();

            var menu = new MatMenu(this);
            await menu.Select(action);
        }

        protected ILocator TableRowElement(IObject obj) => this.Locator.Locator($"tr[mat-row][data-allors-id='{obj.Id}']");
    }
}
