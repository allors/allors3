// <copyright file="ReadJson.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Newtonsoft
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Allors.Protocol.Json.Api.Read;
    using global::Newtonsoft.Json;
    using global::Newtonsoft.Json.Linq;

    /// <summary>
    /// Decodes read messages after validating the original JSON and request-relative references.
    /// </summary>
    public static class ReadJson
    {
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
                original = Parse(Serialize(request));
            }
            catch (JsonException exception)
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

        private static JsonSerializer Serializer() => new JsonSerializer { DateParseHandling = DateParseHandling.None, MaxDepth = 64 };

        internal static string Serialize(object value)
        {
            using var text = new StringWriter(CultureInfo.InvariantCulture);
            using var writer = new JsonTextWriter(text);
            Serializer().Serialize(writer, value);
            return text.ToString();
        }

        internal static T Deserialize<T>(object value)
        {
            try
            {
                using var text = new StringReader(Serialize(value));
                using var reader = new JsonTextReader(text) { DateParseHandling = DateParseHandling.None, MaxDepth = 64 };
                return Serializer().Deserialize<T>(reader);
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

            // JsonTextReader accepts JavaScript extensions; check JSON grammar before using it.
            new JsonSyntax(json).Validate();
            try
            {
                using var text = new StringReader(json);
                using var reader = new JsonTextReader(text) { DateParseHandling = DateParseHandling.None, MaxDepth = 64 };
                var token = JToken.Load(reader, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
                return Normalize(token, "$");
            }
            catch (JsonException exception)
            {
                throw new FormatException("$: Supply strict JSON with unique property names and supported values.", exception);
            }
        }

        internal static object ParseReceipt(string json, string readPart = null, string mutationField = "push")
        {
            if (json == null) throw new FormatException("$: Supply a JSON mutation receipt.");
            new JsonSyntax(json, true).Validate();
            try
            {
                using var text = new StringReader(json);
                using var reader = new JsonTextReader(text) { DateParseHandling = DateParseHandling.None, MaxDepth = 64 };
                if (!reader.Read() || reader.TokenType != JsonToken.StartObject) throw new FormatException("$: Supply a PushRead response object.");
                var result = new Dictionary<string, object>(StringComparer.Ordinal);
                while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                {
                    if (reader.TokenType != JsonToken.PropertyName) throw new FormatException("$: Supply a JSON response property.");
                    var name = (string)reader.Value;
                    if (result.ContainsKey(name)) throw new FormatException("$." + name + ": Remove the duplicate JSON property.");
                    reader.Read();
                    if (name == "i" || name == "sc" || name == mutationField || name == readPart)
                    {
                        var token = JToken.ReadFrom(reader, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
                        result.Add(name, Normalize(token, "$." + name));
                    }
                    else
                    {
                        reader.Skip();
                        result.Add(name, null);
                    }
                }
                return result;
            }
            catch (JsonException exception)
            {
                throw new FormatException("$: Supply a valid mutation receipt with unique property names.", exception);
            }
        }

        private static object Normalize(JToken value, string path)
        {
            switch (value.Type)
            {
                case JTokenType.Object:
                    var properties = new Dictionary<string, object>(StringComparer.Ordinal);
                    foreach (var property in ((JObject)value).Properties())
                    {
                        properties.Add(property.Name, Normalize(property.Value, path + "." + property.Name));
                    }

                    return properties;
                case JTokenType.Array:
                    var items = new List<object>();
                    foreach (var item in value)
                    {
                        items.Add(Normalize(item, path + "[" + items.Count + "]"));
                    }

                    return items;
                case JTokenType.String:
                    return (string)value;
                case JTokenType.Integer:
                case JTokenType.Float:
                    var spelling = Convert.ToString(((JValue)value).Value, CultureInfo.InvariantCulture);
                    if (long.TryParse(spelling, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer))
                    {
                        return integer;
                    }

                    if (!double.TryParse(spelling, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) || double.IsNaN(number) || double.IsInfinity(number))
                    {
                        throw new FormatException(path + ": Supply a finite JSON number.");
                    }

                    if (number >= -9007199254740991 && number <= 9007199254740991 && Math.Truncate(number) == number)
                    {
                        return (long)number;
                    }

                    return number;
                case JTokenType.Boolean:
                    return (bool)value;
                case JTokenType.Null:
                    return null;
                default:
                    throw new FormatException(path + ": Supply a supported JSON value.");
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

        private sealed class JsonSyntax
        {
            private readonly string json;
            private readonly bool receiptOnly;
            private int position;

            internal JsonSyntax(string json, bool receiptOnly = false)
            {
                this.json = json;
                this.receiptOnly = receiptOnly;
            }

            internal void Validate()
            {
                this.Value(0);
                this.WhiteSpace();
                if (this.position != this.json.Length)
                {
                    this.Invalid();
                }
            }

            private void Value(int depth, bool independent = false)
            {
                this.WhiteSpace();
                if (this.position == this.json.Length || depth > 64)
                {
                    this.Invalid();
                }

                var token = this.json[this.position];
                if (token == '{' || token == '[')
                {
                    if (depth == 64)
                    {
                        this.Invalid();
                    }

                    var close = token == '{' ? '}' : ']';
                    ++this.position;
                    this.WhiteSpace();
                    if (this.Take(close))
                    {
                        return;
                    }

                    do
                    {
                        var childIndependent = independent;
                        if (token == '{')
                        {
                            var start = this.position;
                            this.String(independent);
                            if (depth == 0 && this.receiptOnly)
                            {
                                var property = JsonConvert.DeserializeObject<string>(this.json.Substring(start, this.position - start));
                                childIndependent = property == "changed" || property == "discovery";
                            }
                            this.WhiteSpace();
                            if (!this.Take(':'))
                            {
                                this.Invalid();
                            }
                        }

                        this.Value(depth + 1, childIndependent);
                        this.WhiteSpace();
                        if (this.Take(close))
                        {
                            return;
                        }

                        if (!this.Take(','))
                        {
                            this.Invalid();
                        }

                        this.WhiteSpace();
                    }
                    while (true);
                }

                if (token == '"')
                {
                    this.String(independent);
                    return;
                }

                foreach (var literal in new[] { "true", "false", "null" })
                {
                    if (this.position + literal.Length <= this.json.Length && string.CompareOrdinal(this.json, this.position, literal, 0, literal.Length) == 0)
                    {
                        this.position += literal.Length;
                        return;
                    }
                }

                var numberStart = this.position;
                this.Take('-');
                if (!this.Take('0'))
                {
                    if (!this.Digit(true))
                    {
                        this.Invalid();
                    }

                    while (this.Digit(false))
                    {
                    }
                }

                if (this.Take('.'))
                {
                    if (!this.Digit(false))
                    {
                        this.Invalid();
                    }

                    while (this.Digit(false))
                    {
                    }
                }

                if (this.Take('e') || this.Take('E'))
                {
                    if (!this.Take('+'))
                    {
                        this.Take('-');
                    }

                    if (!this.Digit(false))
                    {
                        this.Invalid();
                    }

                    while (this.Digit(false))
                    {
                    }
                }

                var spelling = this.json.Substring(numberStart, this.position - numberStart);
                if (!independent && double.TryParse(spelling, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) && !double.IsInfinity(number))
                {
                    RejectRoundedInteger(spelling, number, "$");
                }
            }

            private void String(bool independent = false)
            {
                if (!this.Take('"'))
                {
                    this.Invalid();
                }

                while (this.position < this.json.Length)
                {
                    var character = this.json[this.position++];
                    if (character == '"')
                    {
                        return;
                    }

                    if (character < 32)
                    {
                        this.Invalid();
                    }

                    if (char.IsHighSurrogate(character))
                    {
                        if (this.position == this.json.Length || !char.IsLowSurrogate(this.json[this.position++]))
                        {
                            this.Invalid();
                        }
                    }
                    else if (char.IsLowSurrogate(character))
                    {
                        this.Invalid();
                    }

                    if (character == '\\')
                    {
                        if (this.position == this.json.Length)
                        {
                            this.Invalid();
                        }

                        var escape = this.json[this.position++];
                        if (escape == 'u')
                        {
                            var code = this.UnicodeEscape();
                            if (!independent && char.IsHighSurrogate(code))
                            {
                                if (!this.Take('\\') || !this.Take('u') || !char.IsLowSurrogate(this.UnicodeEscape()))
                                {
                                    this.Invalid();
                                }
                            }
                            else if (!independent && char.IsLowSurrogate(code))
                            {
                                this.Invalid();
                            }
                        }
                        else if ("\"\\/bfnrt".IndexOf(escape) < 0)
                        {
                            this.Invalid();
                        }
                    }
                }

                this.Invalid();
            }

            private char UnicodeEscape()
            {
                var start = this.position;
                for (var i = 0; i < 4; ++i)
                {
                    if (this.position == this.json.Length || !Uri.IsHexDigit(this.json[this.position++]))
                    {
                        this.Invalid();
                    }
                }

                return (char)int.Parse(this.json.Substring(start, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            private bool Digit(bool nonzero)
            {
                if (this.position < this.json.Length && this.json[this.position] >= (nonzero ? '1' : '0') && this.json[this.position] <= '9')
                {
                    ++this.position;
                    return true;
                }

                return false;
            }

            private bool Take(char character)
            {
                if (this.position < this.json.Length && this.json[this.position] == character)
                {
                    ++this.position;
                    return true;
                }

                return false;
            }

            private void WhiteSpace()
            {
                while (this.position < this.json.Length && " \t\r\n".IndexOf(this.json[this.position]) >= 0)
                {
                    ++this.position;
                }
            }

            private void Invalid() => throw new FormatException("$: Supply strict JSON without comments, duplicate separators or extensions (character " + this.position + ").");
        }
    }
}
