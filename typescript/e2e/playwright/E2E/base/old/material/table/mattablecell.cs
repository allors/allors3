// <copyright file="MatTableCell.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    using System.Threading.Tasks;
    using Microsoft.Playwright;
    using Tests;

    public class MatTableCell
    {
        public MatTableCell(IPage page, ILocator element)
        {
            this.Page = page;
            this.Element = element;
        }

        public IPage Page { get; }

        public ILocator Element { get; }

        public async Task Click()
        {
            await this.Page.WaitForAngular();
            await this.Element.ClickAsync();
        }
    }

    public class MatTableCell<T> : MatTableCell where T : Component
    {
        public MatTableCell(T page, ILocator element)
            : base(page.Page, element) =>
                this.Page = page;

        public new T Page { get; }

        public new async Task<T> Click()
        {
            await base.Click();
            return this.Page;
        }
    }
}
