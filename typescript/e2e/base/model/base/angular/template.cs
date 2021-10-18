// <copyright file="Template.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Newtonsoft.Json.Linq;

namespace Autotest.Angular
{
    using System;
    using System.Linq;
    using Html;

    public partial class Template
    {
        public Template(Directive directive, JToken json)
        {
            this.Directive = directive;
            this.Json = json;
        }

        public Directive Directive { get; }

        public Element[] FlattenedElements { get; set; }

        public INode[] Html { get; set; }

        public JToken Json { get; }

        public string Url { get; set; }

        public void BaseLoad()
        {
            this.Url = this.Json["url"]?.Value<string>();

            var jsonHtml = this.Json["html"];
            this.Html = jsonHtml != null ? jsonHtml.Select(v =>
                {
                    var node = NodeFactory.Create(v, this, null);
                    node.BaseLoad();
                    return node;
                }).ToArray() : Array.Empty<INode>();

            this.FlattenedElements = this.Html.OfType<Element>().SelectMany(v => v.FlattenedElements).ToArray();
        }
    }
}
