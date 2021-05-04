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
        public static object ToJson(object value) =>
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

        public static object FromJson(int tag, object value)
        {
            if (value == null)
            {
                return null;
            }

            return tag switch
            {
                UnitTags.DateTime => value,
                UnitTags.Binary => Convert.FromBase64String((string)value),
                UnitTags.Boolean => value,
                UnitTags.Decimal => XmlConvert.ToDecimal((string)value),
                UnitTags.Float => Convert.ToDouble(value),
                UnitTags.Integer => Convert.ToInt32(value),
                UnitTags.String => value,
                UnitTags.Unique => XmlConvert.ToGuid((string)value),
                _ => throw new Exception($"Unknown unit type with tag {tag}")
            };
        }
    }
}
