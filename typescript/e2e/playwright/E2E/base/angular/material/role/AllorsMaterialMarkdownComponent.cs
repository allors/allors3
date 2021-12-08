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

    public class AllorsMaterialMarkdownComponent : RoleComponent
    {
        public AllorsMaterialMarkdownComponent(IComponent container, IRoleType roleType) : base(container, roleType, "a-mat-markdown")
        {
        }

        public ILocator TextAreaLocator => this.Locator.Locator("textarea[data-allors-id]");

        public async Task<object> GetAsync()
        {
            await this.Page.WaitForAngular();
            return await this.TextAreaLocator.EvaluateAsync<string>(@"(element) => element.easyMDE.value()");
        }

        public async Task SetAsync(string value)
        {
            await this.Page.WaitForAngular();

            await this.TextAreaLocator.EvaluateAsync(@"(element, value) => element.easyMDE.value(value)", value);
        }
    }
}
