// <copyright file="Component.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E
{
    using Database.Meta;
    using Microsoft.Playwright;

    public class ContainerComponent : IComponent
    {
        private readonly string selector;

        protected ContainerComponent(IComponent container, string selector = null)
        {
            this.selector = selector;
            this.Container = container;
        }

        public IComponent Container { get; }

        public IPage Page => this.Container.Page;

        public MetaPopulation M => this.Container.M;

        public ILocator Locator => !string.IsNullOrWhiteSpace(this.selector)
            ? this.Container.Locator.Locator(this.selector)
            : this.Container.Locator;
    }
}
