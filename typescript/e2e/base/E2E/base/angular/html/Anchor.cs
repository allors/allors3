// <copyright file="Anchor.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Html
{
    public class Anchor : HtmlComponent
    {
        public Anchor(IComponent container, string selector)
            : base(container, selector)
        {
        }

        public Anchor(IComponent container, string kind, string value)
            : base(container, kind.ToLowerInvariant() switch
            {
                "innertext" => $"a[normalize-space()='{value}']",
                "routerlink" => $"a[@ng-reflect-router-link='{value}']",
                _ => "a"
            })
        { }
    }
}
