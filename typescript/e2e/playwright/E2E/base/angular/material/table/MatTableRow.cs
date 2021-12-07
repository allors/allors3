// <copyright file="MatTableRow.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Table
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Playwright;

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

            var result = new List<MatTableCell>();
            for (var i = 0; i < await cells.CountAsync(); i++)
            {
                var cell = cells.Nth(i);
                var matTableCell = new MatTableCell(this.Page, cell);
                result.Add(matTableCell);
            }

            return result.ToArray();
        }

        protected ILocator TableCell(string name)
        {
            var cellPath = $"td.mat-column-{name}";
            return this.Element.Locator(cellPath);
        }
    }
}
