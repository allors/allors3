// <copyright file="Convert.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using System;
    using System.Globalization;
    using System.Xml;

    public static class UnitConvert
    {
        public static string ToString(object value) =>
            value switch
            {
                DateTime dateTime => dateTime.ToString("o"),
                byte[] binary => Convert.ToBase64String(binary),
                bool @bool => @bool ? "true" : "false",
                decimal @decimal => @decimal.ToString(CultureInfo.InvariantCulture),
                double @double => @double.ToString(CultureInfo.InvariantCulture),
                int @int => @int.ToString(CultureInfo.InvariantCulture),
                string @string => @string,
                Guid @guid => @guid.ToString("D"),
                null => null,
                _ => throw new ArgumentException()
            };

        public static object FromString(int tag, string value)
        {
            if (value == null)
            {
                return null;
            }

            return tag switch
            {
                UnitTags.DateTime => XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Utc),
                UnitTags.Binary => Convert.FromBase64String(value),
                UnitTags.Boolean => XmlConvert.ToBoolean(value),
                UnitTags.Decimal => XmlConvert.ToDecimal(value),
                UnitTags.Float => XmlConvert.ToDouble(value),
                UnitTags.Integer => XmlConvert.ToInt32(value),
                UnitTags.String => value,
                UnitTags.Unique => XmlConvert.ToGuid(value),
                _ => throw new Exception($"Unknown unit type with tag {tag}")
            };
        }
    }
}
