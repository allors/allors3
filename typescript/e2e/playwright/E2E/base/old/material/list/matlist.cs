// <copyright file="MatList.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    
    using System.Threading.Tasks;
    using Allors.Database;
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Tests;

    public class MatList : SelectorComponent
    {
        public MatList(IPage page, MetaPopulation m, string selector = null)
            : base(page, m) =>
            this.Selector = selector;

        public override string Selector { get; }

        public async Task<MatListItem> FindListItem(IObject obj)
        {
            var listItem = await this.ListItemElement(obj);
            return new MatListItem(this.Page, listItem);
        }

        protected async Task<ILocator> ListItemElement(IObject obj)
        {
            await this.Page.WaitForAngular();

            var itemPath = $"mat-list-item[data-allors-id='{obj.Id}']";
            var path = this.Selector != null ? $"{this.Selector} {itemPath}" : itemPath;
            return this.Page.Locator(path);
        }
    }

    
    public class MatList<T> : MatList where T : Component
    {
        public MatList(T page, MetaPopulation m, string selector = null)
            : base(page.Page, m, selector) =>
            this.Page = page;

        public new T Page { get; }

        public new async Task<MatListItem<T>> FindListItem(IObject obj)
        {
            var listItem = await this.ListItemElement(obj);
            return new MatListItem<T>(this.Page, listItem);
        }
    }
}
