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

        public static string ToMnemonic(object value) =>
            value switch
            {
                DateTime _ => "t",
                byte[] _ => "#",
                bool _ => "b",
                decimal _ => "d",
                double _ => "f",
                int _ => "i",
                string _ => "s",
                Guid _ => "g",
                _ => throw new ArgumentException()
            };

        public static object FromString(Guid objectTypeId, string value)
        {
            if (value == null)
            {
                return null;
            }

            return objectTypeId switch
            {
                var u when u == UnitIds.DateTime => XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Utc),
                var u when u == UnitIds.Binary => Convert.FromBase64String(value),
                var u when u == UnitIds.Boolean => XmlConvert.ToBoolean(value),
                var u when u == UnitIds.Decimal => XmlConvert.ToDecimal(value),
                var u when u == UnitIds.Float => XmlConvert.ToDouble(value),
                var u when u == UnitIds.Integer => XmlConvert.ToInt32(value),
                var u when u == UnitIds.String => value,
                var u when u == UnitIds.Unique => XmlConvert.ToGuid(value),
                _ => throw new Exception($"Unknown unit type with id {objectTypeId}")
            };
        }

        public static object FromString(string mnemonic, string value) =>
            mnemonic switch
            {
                "t" => XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Utc),
                "#" => Convert.FromBase64String(value),
                "b" => XmlConvert.ToBoolean(value),
                "d" => XmlConvert.ToDecimal(value),
                "f" => XmlConvert.ToDouble(value),
                "i" => XmlConvert.ToInt32(value),
                "s" => value,
                "g" => XmlConvert.ToGuid(value),
                _ => throw new Exception($"Unknown mnemonic {mnemonic}")
            };
    }
}
