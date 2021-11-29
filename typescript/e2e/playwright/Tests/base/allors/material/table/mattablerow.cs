// <copyright file="MatTableRow.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    using System.Threading.Tasks;
    using Microsoft.Playwright;
    using Tests;

    public class MatTableRow
    {
        public MatTableRow(IPage page, ILocator element)
        {
            this.Page = page;
            this.Element = element;
        }

        public IPage Page { get; }

        public ILocator Element { get; }

        public MatTableCell FindCell(string name)
        {
            var cell = this.TableCell(name);
            return new MatTableCell(this.Page, cell);
        }

        public async Task<MatTableCell[]> GetCells()
        {
            var cells = this.Element.Locator("td");
            return cells.Select(v => new MatTableCell(this.Page, v)).ToArray();
        }

        protected ILocator TableCell(string name)
        {
            var cellPath = $"td.mat-column-{name}";
            return this.Element.Locator(cellPath);
        }
    }

    public class MatTableRow<T> : MatTableRow where T : Component
    {
        public MatTableRow(T page, ILocator element)
            : base(page.Page, element) =>
            this.Page = page;

        public new T Page { get; }
    }
}
