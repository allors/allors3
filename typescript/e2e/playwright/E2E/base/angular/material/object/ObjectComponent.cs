// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Object
{
    using Database;
    using Database.Meta;
    using Microsoft.Playwright;

    public abstract class ObjectComponent<T> : IComponent where T : IObject
    {
        protected ObjectComponent(IComponent container, T @object, string elementName)
        {
            this.Container = container;
            this.Object = @object;
            this.Locator = this.Page.Locator($"{elementName}:has([data-allors-id='{@object.Id}'])");
        }

        public IComponent Container { get; }

        public IPage Page => this.Container.Page;

        public MetaPopulation M => this.Container.M;

        public ILocator Locator { get; }

        public T Object { get; }
    }
}
