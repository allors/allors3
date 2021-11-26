// <copyright file="Method.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Newtonsoft.Json.Linq;

namespace Autotest.Typescript
{
    using System;
    using System.Linq;

    public class Method : IMember
    {
        public Method(JToken json) => this.Json = json;

        public JToken Json { get; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string[] Decorators { get; set; }

        public Parameter[] Parameters { get; set; }

        public void BaseLoad()
        {
            this.Name = this.Json["name"]?.Value<string>();
            this.Type = this.Json["type"]?.Value<string>();

            var jsonDecorators = this.Json["decorators"];
            this.Decorators = jsonDecorators != null
                ? jsonDecorators.Select(v => v.Value<string>()).ToArray()
                : Array.Empty<string>();

            var jsonParameters = this.Json["parameters"];
            this.Parameters = jsonParameters != null ? jsonParameters.Select(v =>
            {
                var parameter = new Parameter(v);
                parameter.BaseLoad();
                return parameter;
            }).ToArray() : Array.Empty<Parameter>();
        }
    }
}
