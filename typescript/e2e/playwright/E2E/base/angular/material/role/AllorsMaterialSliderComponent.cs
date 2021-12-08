// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Role
{
    using Database.Meta;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    public class AllorsMaterialSliderComponent : RoleComponent
    {
        public AllorsMaterialSliderComponent(IComponent container, RoleType roleType) : base(container, roleType, "a-mat-slider")
        {
        }

        public ILocator SliderLocator => this.Locator.Locator("mat-slider");

        public async Task SetAsync(int min, int max, int value)
        {
            await this.Page.WaitForAngular();

            await this.SliderLocator.ScrollIntoViewIfNeededAsync();

            var boundingBox = await this.SliderLocator.BoundingBoxAsync();
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
}
