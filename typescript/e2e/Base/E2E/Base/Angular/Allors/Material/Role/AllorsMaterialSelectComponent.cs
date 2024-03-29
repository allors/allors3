// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Role
{
    using System.Threading.Tasks;
    using Database;
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    public class AllorsMaterialSelectComponent : RoleComponent
    {
        public AllorsMaterialSelectComponent(IComponent container, IRoleType roleType) : base(container, roleType, "a-mat-select")
        {
        }

        public ILocator MatSelectLocator => this.Locator.Locator("mat-select");

        public ILocator ArrowLocator => this.Locator.Locator(".mat-select-arrow");

        public ILocator ValueTextLocator => this.Locator.Locator(".mat-select-value-text");

        public async Task<string> GetAsync()
        {
            await this.Page.WaitForAngular();
            return await this.ValueTextLocator.TextContentAsync();
        }

        public async Task SetAsync(IObject @object)
        {
            await this.Page.WaitForAngular();
            await this.ArrowLocator.ClickAsync();

            await this.Page.WaitForAngular();
            var optionLocator = this.Page.Locator($"mat-option[data-allors-option-id='{@object.Id}'] span");
            await optionLocator.ClickAsync();
        }

        public async Task SelectAsync(int index)
        {
            await this.Page.WaitForAngular();
            await this.ArrowLocator.ClickAsync();

            await this.Page.WaitForAngular();
            var optionLocator = this.Page.Locator($"mat-option[data-allors-option-id]:nth-of-type({index + 1}) span");
            await optionLocator.ClickAsync();

            var multiple = await this.Locator.GetPropertyAsync<bool?>("multiple");
            if (multiple ?? false)
            {
                await this.Page.Keyboard.PressAsync("Escape");
            }
        }

        public async Task SelectAsync(IObject @object)
        {
            await this.Page.WaitForAngular();
            await this.ArrowLocator.ClickAsync();

            await this.Page.WaitForAngular();
            var optionLocator = this.Page.Locator($"mat-option[data-allors-option-id='{@object?.Id ?? 0}'] span");
            await optionLocator.ClickAsync();

            var multiple = await this.Locator.GetPropertyAsync<bool?>("multiple");
            if (multiple ?? false)
            {
                await this.Page.Keyboard.PressAsync("Escape");
            }
        }
    }
}
