// <copyright file="Convert.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.RestSharp
{
    using System;
    using System.Globalization;
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

        public object FromJson(int tag, object value) =>
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
                    _ => throw new Exception($"Unknown unit type with tag {tag}"),
                },
                _ => value,
            };
    }
}
