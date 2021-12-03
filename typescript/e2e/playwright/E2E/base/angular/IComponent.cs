// <copyright file="Component.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Angular.Components
{
    using Allors.Database.Meta;
    using Microsoft.Playwright;

    public interface IComponent
    {
        IComponent Container { get; }

        IPage Page { get; }

        MetaPopulation M { get; }

        ILocator Locator { get; }
    }
}
