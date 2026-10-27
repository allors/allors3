// <copyright file="Convert.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Newtonsoft
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
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

        public object UnitFromJson(string tag, object value) =>
            value switch
            {
                null => null,
                string stringValue => tag switch
                {
                    UnitTags.DateTime => XmlConvert.ToDateTime(stringValue, XmlDateTimeSerializationMode.Utc),
                    UnitTags.Binary => Convert.FromBase64String(stringValue),
                    UnitTags.Boolean => XmlConvert.ToBoolean(stringValue),
                    UnitTags.Decimal => XmlConvert.ToDecimal(stringValue),
                    UnitTags.Float => XmlConvert.ToDouble(stringValue),
                    UnitTags.Integer => XmlConvert.ToInt32(stringValue),
                    UnitTags.String => value,
                    UnitTags.Unique => XmlConvert.ToGuid(stringValue),
                    _ => throw CreateConversionException(tag, value),
                },
                int intValue => tag switch
                {
                    UnitTags.Decimal => Convert.ToDecimal(intValue),
                    UnitTags.Float => Convert.ToDouble(intValue),
                    UnitTags.Integer => intValue,
                    _ => throw CreateConversionException(tag, value),
                },
                long longValue => tag switch
                {
                    UnitTags.Decimal => Convert.ToDecimal(longValue),
                    UnitTags.Float => Convert.ToDouble(longValue),
                    UnitTags.Integer => Convert.ToInt32(longValue),
                    _ => throw CreateConversionException(tag, value),
                },
                float floatValue => tag switch
                {
                    UnitTags.Decimal => Convert.ToDecimal(floatValue),
                    UnitTags.Float => Convert.ToDouble(floatValue),
                    _ => throw CreateConversionException(tag, value),
                },
                double doubleValue => tag switch
                {
                    UnitTags.Decimal => Convert.ToDecimal(doubleValue),
                    UnitTags.Float => doubleValue,
                    _ => throw CreateConversionException(tag, value),
                },
                DateTime dateValue => tag switch
                {
                    UnitTags.DateTime => dateValue,
                    _ => throw CreateConversionException(tag, value),
                },
                bool boolValue => tag switch
                {
                    UnitTags.Boolean => boolValue,
                    _ => throw CreateConversionException(tag, value),
                },
                _ => value,
            };

        public long? LongFromJson(object value) => (long?)value;

        public long[] LongArrayFromJson(object value) => value switch
        {
            null => null,
            IEnumerable _ => ((IEnumerable<object>)value).Select(v => (long)v).ToArray(),
            _ => new[] { (long)value }
        };

        public string StringFromJson(object value) => (string)value;

        private static Exception CreateConversionException(string tag, object value)
        {
            var unitName = tag switch
            {
                UnitTags.Binary => UnitNames.Binary,
                UnitTags.Boolean => UnitNames.Boolean,
                UnitTags.DateTime => UnitNames.DateTime,
                UnitTags.Decimal => UnitNames.Decimal,
                UnitTags.Float => UnitNames.Float,
                UnitTags.Integer => UnitNames.Integer,
                UnitTags.String => UnitNames.String,
                UnitTags.Unique => UnitNames.Unique,
                _ => "unknown unit",
            };

            return new Exception($"Cannot convert {value.GetType().FullName} to {unitName} (tag '{tag}').");
        }
    }
}
