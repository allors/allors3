// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Angular.Components
{
    using System.Threading.Tasks;
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Tests;
    using Task = System.Threading.Tasks.Task;

    public class AllorsMaterialTextareaComponent : RoleControl
    {
        public AllorsMaterialTextareaComponent(IComponent container, RoleType roleType) : base(container, roleType, "a-mat-textarea")
        {
        }

        public ILocator TextareaLocator => this.Locator.Locator("textarea");

        public async Task<string> GetAsync()
        {
            await this.Page.WaitForAngular();
            return await this.TextareaLocator.InputValueAsync();
        }

        public async Task SetAsync(string value)
        {
            await this.Page.WaitForAngular();
            await this.TextareaLocator.FillAsync(value);
        }
    }
}
