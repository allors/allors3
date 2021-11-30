// <copyright file="Component.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Angular.Components
{
    using Allors.Database.Meta;
    using Microsoft.Playwright;

    public class AnnotatedComponent : IComponent
    {
        protected AnnotatedComponent(IComponent container, string componentType)
        {
            this.Container = container;
            this.Locator = this.Page.Locator($"ng-component[data-allors-component-type='{componentType}']");
        }

        public IComponent Container { get; }

        public IPage Page => this.Container.Page;

        public MetaPopulation M => this.Container.M;

        public ILocator Locator { get; }
    }
}
