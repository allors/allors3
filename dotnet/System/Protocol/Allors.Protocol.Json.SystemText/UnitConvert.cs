// <copyright file="Convert.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.SystemTextJson
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text.Json;
    using System.Xml;

    public class UnitConvert : IUnitConvert
    {
        public object ToJson(object value) =>
            value switch
            {
                DateTime dateTime => dateTime,
                byte[] binary => Convert.ToBase64String(binary),
                bool @bool => @bool,
                decimal @decimal => @decimal.ToString(CultureInfo.InvariantCulture),
                double @double => @double,
                int @int => @int,
                string @string => @string,
                Guid @guid => @guid.ToString("D"),
                null => null,
                _ => throw new ArgumentException()
            };

        public object UnitFromJson(string tag, object value)
        {
            switch (value)
            {
                case null:
                    return null;
                case string @string:
                    return tag switch
                    {
                        UnitTags.DateTime => XmlConvert.ToDateTime(@string, XmlDateTimeSerializationMode.Utc),
                        UnitTags.Binary => Convert.FromBase64String(@string),
                        UnitTags.Boolean => XmlConvert.ToBoolean(@string),
                        UnitTags.Decimal => XmlConvert.ToDecimal(@string),
                        UnitTags.Float => XmlConvert.ToDouble(@string),
                        UnitTags.Integer => XmlConvert.ToInt32(@string),
                        UnitTags.String => @string,
                        UnitTags.Unique => XmlConvert.ToGuid(@string),
                        _ => throw UnitConversionErrors.Unsupported(tag, typeof(string).FullName)
                    };
                // ToJson leaves these CLR values unchanged, so a caller that converts without
                // serializing gets them back as they are; UnitConversionWithoutSerialization pins this.
                case DateTime dateTime:
                    return dateTime;
                case bool @bool:
                    return @bool;
                case double @double:
                    return @double;
                case int @int:
                    return @int;
                default:
                {
                    var element = (JsonElement)value;
                    switch (element.ValueKind)
                    {
                        case JsonValueKind.Null:
                        case JsonValueKind.Undefined:
                            return null;
                        case JsonValueKind.False:
                        case JsonValueKind.True:
                            return tag == UnitTags.Boolean ? element.GetBoolean() : throw CreateConversionException(tag, element);
                        case JsonValueKind.String:
                            return tag switch
                            {
                                UnitTags.DateTime => element.GetDateTime(),
                                UnitTags.Binary => Convert.FromBase64String(element.GetString()),
                                UnitTags.Decimal => Convert.ToDecimal(element.GetString(), CultureInfo.InvariantCulture),
                                UnitTags.String => element.GetString(),
                                UnitTags.Unique => element.GetGuid(),
                                _ => throw CreateConversionException(tag, element)
                            };
                        case JsonValueKind.Number:
                            return tag switch
                            {
                                UnitTags.Decimal => element.GetDecimal(),
                                UnitTags.Float => element.GetDouble(),
                                UnitTags.Integer => element.TryGetInt32(out var integer)
                                    ? integer
                                    : throw UnitConversionErrors.NotAnInteger(Source(element), element.GetRawText()),
                                _ => throw CreateConversionException(tag, element)
                            };
                        default:
                            throw CreateConversionException(tag, element);
                    }
                }
            }
        }

        private static Exception CreateConversionException(string tag, JsonElement element) =>
            UnitConversionErrors.Unsupported(tag, Source(element));

        private static string Source(JsonElement element) =>
            element.ValueKind switch
            {
                JsonValueKind.False => "JSON Boolean",
                JsonValueKind.True => "JSON Boolean",
                _ => $"JSON {element.ValueKind}"
            };


        public long? LongFromJson(object value) =>
            value switch
            {
                null => null,
                JsonElement element => element.GetInt64(),
                _ => (long?)value
            };

        public long[] LongArrayFromJson(object value) =>
            value switch
            {
                null => null,
                long longValue => new[] { longValue },
                JsonElement element => element.ValueKind switch
                {
                    JsonValueKind.Null => null,
                    JsonValueKind.Undefined => null,
                    JsonValueKind.Array => element.EnumerateArray().Select(v => v.GetInt64()).ToArray(),
                    JsonValueKind.Number => new[] { element.GetInt64() },
                },
                _ => (long[])value
            };

        public string StringFromJson(object value) => value switch
        {
            null => null,
            JsonElement element => element.GetString(),
            _ => (string)value
        };
    }
}
