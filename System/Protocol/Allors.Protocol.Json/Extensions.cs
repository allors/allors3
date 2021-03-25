// <copyright file="FromJsonVisitor.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Xml;
    using System.Xml.Serialization;
    using Procedure = Allors.Protocol.Json.Data.Procedure;
    using Pull = Allors.Protocol.Json.Data.Pull;

    public static class Extensions
    {
        public static IDictionary<string, object> FromJsonForValueByName(this string[][] valueByName) =>
            valueByName?.Select(namedCollection =>
            {
                var key = namedCollection[0];

                if (namedCollection.Length <= 1)
                {
                    return new KeyValuePair<string, object>(key, null);
                }

                var mnemonic = namedCollection[1];
                var value = UnitConvert.FromString(mnemonic, namedCollection[2]);

                return new KeyValuePair<string, object>(key, value);
            }).ToDictionary(v => v.Key, v => v.Value);

        public static string[][] ToJsonForValueByName(this IDictionary<string, object> valueByName) =>
            valueByName?.Select(kvp =>
            {
                var name = kvp.Key;
                var value = kvp.Value;

                return value != null ? new[] { name, UnitConvert.ToMnemonic(value), UnitConvert.ToString(value) } : new[] { name };
            }).ToArray();
    }
}
