// <copyright file="ReadJson.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.SystemTextJson
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using Allors.Protocol.Json.Api.Read;

    /// <summary>
    /// Decodes read messages after validating the original JSON and request-relative references.
    /// </summary>
    public static class ReadJson
    {
        // The writer counts the terminal scalar as a level; the wire reader admits 64 containers.
        private static readonly JsonSerializerOptions SerializationOptions = new JsonSerializerOptions { MaxDepth = 65 };

        public static ReadRequest DeserializeRequest(string json)
        {
            var value = Parse(json);
            ReadValidation.ValidateRequest(value);
            return Deserialize<ReadRequest>(value);
        }

        public static object DeserializeResponse(string json, ReadRequest request)
        {
            var value = Parse(json);
            object original;
            try
            {
                original = Parse(JsonSerializer.Serialize(request, SerializationOptions));
            }
            catch (Exception exception) when (exception is JsonException || exception is NotSupportedException)
            {
                throw new FormatException("$request: Supply an acyclic request containing only supported JSON values.", exception);
            }
            ReadValidation.ValidateResponse(value, original);
            var kind = (string)((IDictionary<string, object>)value)["k"];
            return kind switch
            {
                "discovery" => Deserialize<DiscoverySuccess>(value),
                "complete" => Deserialize<ReadSuccess>(value),
                _ => Deserialize<ReadFailure>(value),
            };
        }

        internal static T Deserialize<T>(object value)
        {
            try
            {
                // Canonical numbers make integral spellings such as 1e0 work for integer DTO fields.
                return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, SerializationOptions));
            }
            catch (JsonException exception)
            {
                throw new FormatException("$: Supply a read message with values supported by its declared fields.", exception);
            }
        }

        internal static object Parse(string json)
        {
            if (json == null)
            {
                throw new FormatException("$: Supply a JSON read message, not null input.");
            }

            for (var i = 0; i < json.Length; ++i)
            {
                if (char.IsHighSurrogate(json[i]))
                {
                    if (++i == json.Length || !char.IsLowSurrogate(json[i]))
                    {
                        throw new FormatException("$: Supply valid Unicode strings with paired surrogates.");
                    }
                }
                else if (char.IsLowSurrogate(json[i]))
                {
                    throw new FormatException("$: Supply valid Unicode strings with paired surrogates.");
                }
            }

            try
            {
                using var document = JsonDocument.Parse(json);
                return Normalize(document.RootElement, "$");
            }
            catch (JsonException exception)
            {
                throw new FormatException("$: Supply strict JSON without comments, trailing commas or extra content.", exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new FormatException("$: Supply valid Unicode strings with paired surrogate escapes.", exception);
            }
        }

        internal static object ParseReceipt(string json, string readPart = null, string mutationField = "push")
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                if (document.RootElement.ValueKind != JsonValueKind.Object) throw new FormatException("$: Supply a PushRead response object.");
                var result = new Dictionary<string, object>(StringComparer.Ordinal);
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    if (result.ContainsKey(property.Name)) throw new FormatException("$." + property.Name + ": Remove the duplicate JSON property.");
                    result.Add(property.Name, property.Name == "i" || property.Name == "sc" || property.Name == mutationField || property.Name == readPart ? Normalize(property.Value, "$." + property.Name) : null);
                }
                return result;
            }
            catch (JsonException exception)
            {
                throw new FormatException("$: Supply a syntactically complete JSON mutation receipt.", exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new FormatException("$: Supply valid Unicode strings in the mutation receipt.", exception);
            }
        }

        private static void RejectRoundedInteger(string spelling, double number, string path)
        {
            if (Math.Truncate(number) != number)
            {
                return;
            }

            var exponentOffset = spelling.IndexOfAny(new[] { 'e', 'E' });
            var mantissaEnd = exponentOffset < 0 ? spelling.Length : exponentOffset;
            var exponent = 0;
            if (exponentOffset >= 0)
            {
                var exponentText = spelling.Substring(exponentOffset + 1);
                if (!int.TryParse(exponentText, System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out exponent))
                {
                    exponent = exponentText[0] == '-' ? int.MinValue : int.MaxValue;
                }
            }

            var digits = 0;
            var decimalPosition = -1;
            var lastNonzero = 0;
            for (var i = spelling[0] == '-' ? 1 : 0; i < mantissaEnd; ++i)
            {
                if (spelling[i] == '.')
                {
                    decimalPosition = digits;
                }
                else
                {
                    ++digits;
                    if (spelling[i] != '0')
                    {
                        lastNonzero = digits;
                    }
                }
            }

            if (lastNonzero != 0 && (long)(decimalPosition < 0 ? digits : decimalPosition) + exponent < lastNonzero)
            {
                throw new FormatException(path + ": Reduce numeric precision so the fractional part is not rounded to an integer.");
            }
        }

        private static object Normalize(JsonElement value, string path)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.Object:
                    var properties = new Dictionary<string, object>(StringComparer.Ordinal);
                    foreach (var property in value.EnumerateObject())
                    {
                        var propertyPath = path + "." + property.Name;
                        if (properties.ContainsKey(property.Name))
                        {
                            throw new FormatException(propertyPath + ": Remove the duplicate JSON property.");
                        }

                        properties.Add(property.Name, Normalize(property.Value, propertyPath));
                    }

                    return properties;
                case JsonValueKind.Array:
                    var items = new List<object>();
                    foreach (var item in value.EnumerateArray())
                    {
                        items.Add(Normalize(item, path + "[" + items.Count + "]"));
                    }

                    return items;
                case JsonValueKind.String:
                    return value.GetString();
                case JsonValueKind.Number:
                    if (value.TryGetInt64(out var integer))
                    {
                        return integer;
                    }

                    var number = value.GetDouble();
                    if (double.IsNaN(number) || double.IsInfinity(number))
                    {
                        throw new FormatException(path + ": Supply a finite JSON number.");
                    }

                    RejectRoundedInteger(value.GetRawText(), number, path);

                    if (number >= -9007199254740991 && number <= 9007199254740991 && Math.Truncate(number) == number)
                    {
                        return (long)number;
                    }

                    return number;
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                    return null;
                default:
                    throw new FormatException(path + ": Supply a supported JSON value.");
            }
        }
    }
}
