// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Role
{
    using System.Threading.Tasks;
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    public class AllorsMaterialLocalisedMarkdownComponent : RoleComponent
    {
        public AllorsMaterialLocalisedMarkdownComponent(IComponent container, IRoleType roleType) : base(container, roleType, "a-mat-localised-markdown")
        {
        }

        public ILocator ElementLocator => this.Locator.Locator("textarea[data-allors]");

        public async Task<object> GetAsync()
        {
            await this.Page.WaitForAngular();

            return await this.ElementLocator.EvaluateAsync<string>(@"(element) => element.easyMDE.value()");
        }

        public async Task SetAsync(string value)
        {
            await this.Page.WaitForAngular();

            await this.Locator.ScrollIntoViewIfNeededAsync();

            await this.ElementLocator.EvaluateAsync(@"(element, value) => element.easyMDE.value(value)", value);
        }
    }
}
