// <copyright file="MatTableCell.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Table
{
    using System.Threading.Tasks;
    using Microsoft.Playwright;

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
}
