// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Role
{
    using Allors.Database.Meta;
    using Database;
    using Task = System.Threading.Tasks.Task;

    public class AllorsMaterialRadioGroupComponent : RoleComponent
    {
        public AllorsMaterialRadioGroupComponent(IComponent container, IRoleType roleType) : base(container, roleType, "a-mat-radio-group")
        {
        }

        public async Task SelectAsync(IObject value)
        {
            await this.Page.WaitForAngular();

            var isVisible = await this.Locator.IsVisibleAsync();

            var radio = this.Locator.Locator($"mat-radio-button[data-allors-radio-value='{value.Id}']");
            await radio.ClickAsync();
        }


        public async Task SelectAsync(string value)
        {
            await this.Page.WaitForAngular();

            var isVisible = await this.Locator.IsVisibleAsync();

            var radio = this.Locator.Locator($"mat-radio-button[data-allors-radio-value='{value}']");
            await radio.ClickAsync();
        }
    }
}
