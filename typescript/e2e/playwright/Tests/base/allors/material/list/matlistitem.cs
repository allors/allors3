// <copyright file="MatListItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    
    using System.Threading.Tasks;
    using Microsoft.Playwright;
    using Tests;

    public class MatListItem
    {
        public MatListItem(IPage driver, ILocator element)
        {
            this.Driver = driver;
            this.Element = element;
        }

        public IPage Driver { get; }

        public ILocator Element { get; }

        public async Task Click()
        {
            await this.Driver.WaitForAngular();
            await this.Element.ClickAsync();
        }
    }

    
    public class MatListItem<T> : MatListItem where T : Component
    {
        public MatListItem(T page, ILocator element)
            : base(page.Page, element) =>
            this.Page = page;

        public T Page { get; }

        public new async Task<T> Click()
        {
            await base.Click();
            return this.Page;
        }
    }
}
