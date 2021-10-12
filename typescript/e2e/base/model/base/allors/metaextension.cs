// <copyright file="MetaExtension.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Autotest
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    public partial class MetaExtension
    {
        public string Tag { get; set; }

        public string List { get; set; }

        public string Overview { get; set; }

        public string Create { get; set; }

        public string Edit { get; set; }

        public static void Load(Dictionary<string, MetaExtension> metaExtensionByTag, JArray jsonMetaExtensions, Action<MetaExtension, JToken> setter)
        {
            foreach (var json in jsonMetaExtensions)
            {
                if (json["tag"] != null)
                {
                    var tag = json["tag"].Value<string>();
                    if (tag == null)
                    {
                        throw new ArgumentException("tag is not a string: " + json["tag"].Value<object>());
                    }

                    if (!metaExtensionByTag.TryGetValue(tag, out var metaExtension))
                    {
                        metaExtension = new MetaExtension
                        {
                            Tag = tag,
                        };
                        metaExtensionByTag.Add(tag, metaExtension);
                    }

                    setter(metaExtension, json);
                }
            }
        }
    }
}
