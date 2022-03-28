// <copyright file="Component.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular
{
    using System.Threading.Tasks;
    using Allors.Database.Meta;
    using Info;
    using Microsoft.Playwright;

    public partial class AppRoot : IComponent
    {
        private AppRoot(IPage page, MetaPopulation m, string selector)
        {
            this.Page = page;
            this.M = m;
            this.Locator = this.Page.Locator(selector);
        }

        public static async Task<AppRoot> New(IPage page, MetaPopulation m, string selector)
        {
            var appRoot = new AppRoot(page, m, selector);
            appRoot.ApplicationInfo = await ApplicationInfo.New(appRoot);
            return appRoot;
        }

        public MetaPopulation M { get; set; }

        public IComponent Container => null;

        public IPage Page { get; }

        public ILocator Locator { get; }

        public ApplicationInfo ApplicationInfo { get; private set; }

        public async Task<string> GetAngularVersionAsync() => await this.Locator.GetAttributeAsync("ng-version");

        public async Task<string> GetAllors(string property) => await this.Locator.EvaluateAsync<string>($"(element) => element.allors.{property}");
    }
}
