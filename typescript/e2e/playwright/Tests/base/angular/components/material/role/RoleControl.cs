// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Angular.Components
{
    using Allors.Database.Meta;
    using Microsoft.Playwright;

    public abstract class RoleControl : IComponent
    {
        protected RoleControl(IComponent container, RoleType roleType, string elementName, string componentType)
        {
            this.Container = container;
            this.RoleType = roleType;
            this.Locator = this.Page.Locator($"{elementName}[data-allors-component-type='{componentType}']:has([data-allors-roletype='{roleType.RelationType.Tag}'])");
        }

        public IComponent Container { get; }

        public IPage Page => this.Container.Page;

        public MetaPopulation M => this.Container.M;

        public ILocator Locator { get; }

        public RoleType RoleType { get; }
    }
}
