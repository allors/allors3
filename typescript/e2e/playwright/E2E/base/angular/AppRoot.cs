// <copyright file="Component.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular
{
    using System.Threading.Tasks;
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    public class AppRoot : IComponent
    {
        public AppRoot(IPage page, MetaPopulation m, string selector)
        {
            this.Page = page;
            this.M = m;
            this.Locator = this.Page.Locator(selector);
        }

        public MetaPopulation M { get; set; }

        public IComponent Container => null;

        public IPage Page { get; }

        public ILocator Locator { get; }

        public async Task<string> GetAngularVersionAsync() => await this.Locator.GetAttributeAsync("ng-version");

        public async Task<string> GetAllors(string property) => await this.Locator.EvaluateAsync<string>($"(element) => element.allors.{property}");
    }
}
