// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Html
{
    using System.Threading.Tasks;
    using Allors.Database.Meta;
    using Info;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    public abstract class HtmlComponent : IComponent
    {
        protected HtmlComponent(IComponent container, string selector)
        {
            this.Container = container;
            this.Locator = this.Container.Locator.Locator(selector);
        }

        public IComponent Container { get; }

        public MetaPopulation M => this.Container.M;

        public IPage Page => this.Container.Page;

        public ILocator Locator { get; }

        public ApplicationInfo ApplicationInfo => this.Container.ApplicationInfo;

        public async Task ClickAsync()
        {
            await this.Page.WaitForAngular();
            await this.Locator.ClickAsync();
        }

        public async Task<string> GetAttributeAsync(string attributeName)
        {
            await this.Page.WaitForAngular();
            return await this.Locator.GetAttributeAsync(attributeName);
        }

        public async Task<bool> IsVisibleAsync()
        {
            await this.Page.WaitForAngular();
            return await this.Locator.IsVisibleAsync();
        }

        public async Task<bool> IsCheckedAsync()
        {
            await this.Page.WaitForAngular();
            return await this.Locator.IsCheckedAsync();
        }

        public async Task<bool> IsEnabledAsync()
        {
            await this.Page.WaitForAngular();
            return await this.Locator.IsEnabledAsync();
        }

        public async Task<bool> IsEditableAsync()
        {
            await this.Page.WaitForAngular();
            return await this.Locator.IsEditableAsync();
        }

        public async Task<bool> IsHiddenAsync()
        {
            await this.Page.WaitForAngular();
            return await this.Locator.IsHiddenAsync();
        }
    }
}
