// <copyright file="Convert.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using System;
    using System.Globalization;
    using System.Text.Json;

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

            var jsonElement = (JsonElement)value;

            return jsonElement.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,
                JsonValueKind.False => false,
                JsonValueKind.True => true,
                _ => tag switch
                {
                    UnitTags.DateTime => jsonElement.GetDateTime(),
                    UnitTags.Binary => Convert.FromBase64String(jsonElement.GetString()),
                    UnitTags.Decimal => jsonElement.ValueKind == JsonValueKind.String
                        ? Convert.ToDecimal(jsonElement.GetString(), CultureInfo.InvariantCulture)
                        : jsonElement.GetDecimal(),
                    UnitTags.Float => jsonElement.GetDouble(),
                    UnitTags.Integer => jsonElement.GetInt32(),
                    UnitTags.String => jsonElement.GetString(),
                    UnitTags.Unique => jsonElement.GetGuid(),
                    _ => throw new Exception($"{jsonElement.ValueKind} not supported for tag {tag}")
                }
            };
        }
    }
}
