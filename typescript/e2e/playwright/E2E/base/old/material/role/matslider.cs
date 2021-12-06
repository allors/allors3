// <copyright file="MatSlider.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    using System.Threading.Tasks;
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Tests;
    using Task = System.Threading.Tasks.Task;

    public class MatSlider
    : SelectorComponent
    {
        public MatSlider(IPage page, MetaPopulation m, RoleType roleType, params string[] scopes)
        : base(page, m) =>
            this.Selector = $"a-mat-slider{this.ByScopesPredicate(scopes)} [@data-allors-roletype='{roleType.RelationType.Tag}']//mat-slider";

        public override string Selector { get; }

        public async Task Select(int min, int max, int value)
        {
            await this.Page.WaitForAngular();

            var element = this.Page.Locator(this.Selector);

            await element.ScrollIntoViewIfNeededAsync();

            var boundingBox = await element.BoundingBoxAsync();
            if (boundingBox != null)
            {
                var width = boundingBox.Width;
                var height = boundingBox.Height;
                var offsetX = (value - 1) * width / (max - min);
                var offsetY = height / 2;

                var x = boundingBox.X + offsetX;
                var y = boundingBox.Y + offsetY;

                await this.Page.Mouse.ClickAsync(x, y);
            }
        }
    }


    public class MatSlider<T> : MatSlider where T : Component
    {
        public MatSlider(T page, MetaPopulation m, RoleType roleType, params string[] scopes)
            : base(page.Page, m, roleType, scopes) =>
            this.Page = page;

        public new T Page { get; }

        public new async Task<T> Select(int min, int max, int value)
        {
            await base.Select(min, max, value);
            return this.Page;
        }
    }
}
