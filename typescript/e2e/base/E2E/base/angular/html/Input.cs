// <copyright file="Input.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Html
{
    using System.Threading.Tasks;
    using Task = System.Threading.Tasks.Task;

    public class Input : HtmlComponent
    {
        public Input(IComponent container, string selector)
            : base(container, selector)
        {
        }

        public Input(IComponent container, string kind, string value)
            : base(container, kind.ToLowerInvariant() switch
            {
                "id" => $"input[@id='{value}']",
                "name" => $"input[@name='{value}']",
                "formcontrolname" => $"input[@formcontrolname='{value}']",
                _ => "input"
            })
        { }

        public async Task<string> GetAsync()
        {
            await this.Page.WaitForAngular();
            return await this.Locator.InputValueAsync();
        }

        public async Task SetAsync(string value)
        {
            await this.Page.WaitForAngular();
            await this.Locator.FillAsync(value);
        }
    }
}
