// <copyright file="MatTable.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Database;
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Tests;

    public class MatTable : SelectorComponent
    {
        public MatTable(IPage page, MetaPopulation m, string selector = null)
            : base(page, m) =>
            this.Selector = selector;

        public override string Selector { get; }

        public async Task<string[]> GetObjectIds()
        {
            const string rowPath = "tr[mat-row][data-allors-id]";
            var path = this.Selector != null ? $"{this.Selector} {rowPath}" : rowPath;
            var rows = this.Page.Locator(path);

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

            const string tablePath = "table";
            var path = this.Selector != null ? this.Selector + " " + tablePath : tablePath;
            var table = this.Page.Locator(path);
            var attribute = await table.GetAttributeAsync("data-allors-actions");
            return !string.IsNullOrWhiteSpace(attribute) ? attribute.Split(",") : Array.Empty<string>();
        }

        public MatTableRow FindRow(IObject obj)
        {
            var row = this.TableRowElement(obj);
            return new MatTableRow(this.Page, row);
        }

        public void DefaultAction(IObject obj)
        {
            var row = this.FindRow(obj);
            var cell = row.Cells[1];
            cell.Click();
        }

        public void Action(IObject obj, string action)
        {
            var row = this.FindRow(obj);
            var cell = row.FindCell("menu");
            cell.Click();

            var menu = new MatMenu(this.Page, this.M);
            menu.Select(action);
        }

        protected ILocator TableRowElement(IObject obj)
        {
            var rowPath = $"tr[mat-row][data-allors-id='{obj.Id}']";
            var path = this.Selector != null ? this.Selector + " " + rowPath : rowPath;
            return this.Page.Locator(path);
        }
    }


    public class MatTable<T> : MatTable where T : Component
    {
        public MatTable(T page, MetaPopulation m, string selector = null)
            : base(page.Page, m, selector) =>
            this.Page = page;

        public T Page { get; }

        public new MatTableRow<T> FindRow(IObject obj)
        {
            var row = this.TableRowElement(obj);
            return new MatTableRow<T>(this.Page, row);
        }
    }
}
