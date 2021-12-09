// <copyright file="Component.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E
{
    using Allors.Database.Meta;
    using Microsoft.Playwright;

    public interface IComponent
    {
        MetaPopulation M { get; }

        IComponent Container { get; }

        IPage Page { get; }

        ILocator Locator { get; }
    }
}
