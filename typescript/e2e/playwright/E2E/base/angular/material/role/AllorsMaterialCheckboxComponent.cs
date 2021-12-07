// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Role
{
    using System.Threading.Tasks;
    using Database.Meta;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    public class AllorsMaterialCheckboxComponent : RoleComponent
    {
        public AllorsMaterialCheckboxComponent(IComponent container, RoleType roleType) : base(container, roleType, "a-mat-checkbox")
        {
        }

        public ILocator MatCheckboxLocator => this.Locator.Locator("mat-checkbox");

        public ILocator LabelLocator => this.Locator.Locator("label");

        public ILocator InputLocator => this.Locator.Locator("input");

        public async Task<bool?> GetAsync()
        {
            await this.Page.WaitForAngular();

            bool.TryParse(await this.MatCheckboxLocator.GetAttributeAsync("ng-reflect-indeterminate"), out var indeterminate);

            if (indeterminate)
            {
                return null;
            }

            return await this.InputLocator.IsCheckedAsync();
        }

        public async Task SetAsync(bool value)
        {
            var isChecked = await this.GetAsync();

            if (!isChecked.HasValue)
            {
                // Indeterminate
                await this.LabelLocator.ClickAsync();
                await this.Page.WaitForAngular();

                if (!value)
                {
                    await this.LabelLocator.ClickAsync();
                }
            }
            else if (isChecked.Value)
            {
                if (!value)
                {
                    await this.LabelLocator.ClickAsync();
                }
            }
            else if (value)
            {
                await this.LabelLocator.ClickAsync();
            }
        }
    }
}
