// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Role
{
    using System.Threading.Tasks;
    using Database.Meta;
    using Microsoft.Playwright;

    public class AllorsMaterialStaticComponent : RoleComponent
    {
        public AllorsMaterialStaticComponent(IComponent container, RoleType roleType) : base(container, roleType, "a-mat-static")
        {
        }

        public ILocator InputLocator => this.Locator.Locator("input");

        public async Task<string> GetAsync()
        {
            await this.Page.WaitForAngular();
            return await this.InputLocator.InputValueAsync();
        }
    }
}
