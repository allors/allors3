// <copyright file="Element.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Html
{
    public class Element : HtmlComponent
    {
        public Element(IComponent container, string selector)
            : base(container, selector)
        {
        }
    }
}
