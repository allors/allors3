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

                var type = namedCollection[1];
                var @string = namedCollection[2];
                object value = type switch
                {
                    "s" => @string,
                    "i" => XmlConvert.ToInt32(@string),
                    "f" => XmlConvert.ToDouble(@string),
                    "d" => XmlConvert.ToDecimal(@string),
                    "t" => XmlConvert.ToDateTime(@string, XmlDateTimeSerializationMode.Utc),
                    "g" => XmlConvert.ToGuid(@string),
                    _ => throw new NotSupportedException()
                };


                return new KeyValuePair<string, object>(key, value);
            }).ToDictionary(v => v.Key, v => v.Value);
        
        public static string[][] ToJsonForValueByName(this IDictionary<string, object> valueByName) =>
            valueByName?.Select(kvp =>
            {
                var name = kvp.Key;
                var value = kvp.Value;

                return value != null ? new[] { name, value.ToJsonValueType(), value.ToJsonValue() } : new[] { name };
            }).ToArray();

        public static string ToJsonValueType(this object value) =>
            value switch
            {
                string _ => "s",
                int _ => "i",
                decimal _ => "d",
                double _ => "f",
                DateTime _ => "t",
                Guid _ => "g",
                _ => throw new NotSupportedException()
            };

        public static string ToJsonValue(this object value) =>
            value switch
            {
                string @string => @string,
                int @int => XmlConvert.ToString(@int),
                decimal @decimal => XmlConvert.ToString(@decimal),
                double @double => XmlConvert.ToString(@double),
                DateTime dateTime => XmlConvert.ToString(dateTime, XmlDateTimeSerializationMode.Utc),
                Guid guid => XmlConvert.ToString(guid),
                _ => throw new NotSupportedException()
            };
    }
}
