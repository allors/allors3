// <copyright file="Serialization.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Serialization type.</summary>

namespace Allors.Database.Adapters
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Xml;
    using Meta;

    /// <summary>
    /// Xml tag definitions and utility methods for Xml Serialization.
    /// An <see cref="IDatabase"/> is serialized to a <see cref="XmlDocument"/>
    /// according to the Allors Serialization Xml Schema.
    /// </summary>
    public static class Serialization
    {
        /// <summary>
        /// This is the document element for a serialized <see cref="IDatabase"/>.
        /// </summary>
        public const string Allors = "allors";

        /// <summary>
        /// This attribute holds the version of the Allors Framework when this <see cref="IDatabase"/> was saved.
        /// Attribute of the <see cref="Serialization#Allors"/> element.
        /// </summary>
        public const string Version = "version";

        /// <summary>
        /// The current <see cref="Serialization#Version"/> of the serialization schema.
        /// </summary>
        public const int VersionCurrent = 1;

        /// <summary>
        /// This attribute holds the <see cref="ObjectId"/> of the association of a relation.
        /// Attribute of the <see cref="Serialization#ChangedRelations"/> element.
        /// </summary>
        public const string Association = "a";

        /// <summary>
        /// This attribute is used for <see cref="ObjectType#Id"/> and <see cref="RelationType#Id"/>.
        /// Attribute of the <see cref="Serialization#IObjectType"/> and <see cref="Serialization#RelationType"/> element.
        /// </summary>
        public const string Id = "i";

        /// <summary>
        /// This element is the container for objects and relations.
        /// Child element of the <see cref="Serialization#Allors"/> element.
        /// </summary>
        public const string Population = "population";

        /// <summary>
        /// This element is the container for <see cref="IObject"/>s.
        /// Child element of the <see cref="Serialization#Population"/> element.
        /// </summary>
        public const string Objects = "objects";

        /// <summary>
        /// This element is the container for new <see cref="IObject"/>s.
        /// Child element of the <see cref="Serialization#Objects"/> element.
        /// </summary>
        public const string New = "new";

        /// <summary>
        /// This element is the container for deleted <see cref="IObject"/>s.
        /// Child element of the <see cref="Serialization#Objects"/> element.
        /// </summary>
        public const string Deleted = "deleted";

        /// <summary>
        /// This character is used to group multiple <see cref="ObjectId"/>s into one value.
        /// </summary>
        public const string ObjectsSplitter = ",";

        /// <summary>
        /// This character is used to group multiple <see cref="ObjectId"/>s into one value.
        /// </summary>
        public const string ObjectSplitter = ":";

        /// <summary>
        /// This element groups <see cref="IObject"/>s having the same <see cref="ObjectType"/>.
        /// Child element of the <see cref="Serialization#Objects"/> element.
        /// </summary>
        public const string ObjectType = "ot";

        /// <summary>
        /// This element groups the <see cref="IObject"/>s and relations.
        /// Child element of the <see cref="Serialization#Allors"/> element.
        /// </summary>
        public const string Database = "database";

        /// <summary>
        /// This element groups the <see cref="IObject"/>s and relations.
        /// Child element of the <see cref="Serialization#Allors"/> element.
        /// </summary>
        public const string Workspace = "workspace";

        /// <summary>
        /// This element holds a relation.
        /// Child element of the <see cref="Serialization#RelationType"/> element.
        /// </summary>
        public const string Relation = "r";

        /// <summary>
        /// This element holds a relation.
        /// Child element of the <see cref="Serialization#RelationType"/> element.
        /// </summary>
        public const string NoRelation = "x";

        /// <summary>
        /// This element is the container for relations.
        /// Child element of the <see cref="Serialization#Population"/> element.
        /// </summary>
        public const string Relations = "relations";

        /// <summary>
        /// This element groups relations having the same <see cref="RelationType"/> and
        /// where the role's <see cref="ObjectType"/> is a composite.
        /// Child element of the <see cref="Serialization#Relations"/> element.
        /// </summary>
        public const string RelationTypeComposite = "rtc";

        /// <summary>
        /// This element groups relations having the same <see cref="RelationType"/> and
        /// where the role's <see cref="ObjectType"/> is a unit.
        /// Child element of the <see cref="Serialization#Relations"/> element.
        /// </summary>
        public const string RelationTypeUnit = "rtu";

        /// <summary>
        /// Char array for <see cref="Serialization#ObjectsSplitter"/>.
        /// </summary>
        public static readonly char[] ObjectsSplitterCharArray = { ObjectsSplitter[0] };

        /// <summary>
        /// Char array for <see cref="Serialization#ObjectSplitter"/>.
        /// </summary>
        public static readonly char[] ObjectSplitterCharArray = { ObjectSplitter[0] };

        /// <summary>
        /// UTF-8 that reports bytes that are not UTF-8, where <see cref="Encoding.UTF8"/> replaces them.
        /// </summary>
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);

        /// <summary>
        /// Checks if the <see cref="IDatabase#Version"/> is correct.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public static void CheckVersion(int version)
        {
            if (VersionCurrent != version)
            {
                throw new ArgumentException("Database supports version " + VersionCurrent + " but found version " + version + ".");
            }
        }

        /// <summary>
        /// The exception of a load for a file without a <see cref="Population"/> element.
        /// </summary>
        /// <returns>The exception.</returns>
        public static ArgumentException NoPopulation() =>
            new ArgumentException("The file is not a saved population: it has no <" + Population + "> element.");

        /// <summary>
        /// The exception of a load for an object whose id is not the id of a database object: 0, which denotes no
        /// object, or a negative id, which is the id of a workspace object.
        /// </summary>
        /// <param name="objectId">The object id.</param>
        /// <returns>The exception.</returns>
        public static ArgumentException NoObjectId(long objectId) =>
            new ArgumentException("The file holds an object with the id " + XmlConvert.ToString(objectId) + ": the id of a database object is 1 or higher.");

        /// <summary>
        /// The exception of a load for a relation of an object that the file does not hold.
        /// </summary>
        /// <param name="relationType">The relation type.</param>
        /// <param name="associationId">The id of the association of the relation.</param>
        /// <param name="objectId">The id of the object that the file does not hold: the association or a role.</param>
        /// <returns>The exception.</returns>
        public static ArgumentException NoObject(IRelationType relationType, long associationId, long objectId) =>
            NoObject(relationType.RoleType.ToString(), associationId, objectId);

        /// <summary>
        /// The exception of a load for a relation of an object that the file does not hold, where the relation is of a
        /// relation type that the schema may no longer know.
        /// </summary>
        /// <param name="metaPopulation">The schema.</param>
        /// <param name="relationTypeId">The id of the relation type.</param>
        /// <param name="associationId">The id of the association of the relation.</param>
        /// <param name="objectId">The id of the object that the file does not hold: the association or a role.</param>
        /// <returns>The exception.</returns>
        public static ArgumentException NoObject(IMetaPopulation metaPopulation, Guid relationTypeId, long associationId, long objectId) =>
            metaPopulation.FindById(relationTypeId) is IRelationType relationType
                ? NoObject(relationType, associationId, objectId)
                : NoObject("the relation type " + relationTypeId, associationId, objectId);

        private static ArgumentException NoObject(string relationType, long associationId, long objectId) =>
            new ArgumentException(
                associationId == objectId
                    ? "The file holds a relation of " + relationType + " for the object " + XmlConvert.ToString(objectId) + ", which is not among its objects."
                    : "The file holds a relation of " + relationType + " from the object " + XmlConvert.ToString(associationId) + " to the object " + XmlConvert.ToString(objectId) + ", which is not among its objects.");

        /// <summary>
        /// Reads the unit of a role from its XML value, for a load.
        /// </summary>
        /// <param name="roleType">The role type.</param>
        /// <param name="value">The XML value.</param>
        /// <returns>The unit.</returns>
        /// <exception cref="ArgumentException">
        /// The role type cannot hold the unit as it is: a string or a binary that is longer than its size, a decimal
        /// with more digits than its precision or more decimals than its scale, a float that is NaN or an infinity,
        /// or a string with the character U+0000.
        /// </exception>
        /// <exception cref="FormatException">
        /// The XML value is not the text that a save writes for a unit of the unit type of the role type.
        /// </exception>
        public static object ReadUnit(IRoleType roleType, string value)
        {
            var unit = ReadString(value, ((IUnit)roleType.ObjectType).Tag);

            switch (unit)
            {
                case string @string when roleType.Size > -1 && @string.Length > roleType.Size:
                    throw new ArgumentException($"The string of {@string.Length} characters is longer than the size {roleType.Size} of {roleType}.");

                case decimal @decimal when !roleType.Fits(@decimal):
                    throw new ArgumentException($"The decimal {XmlConvert.ToString(@decimal)} has more digits than the precision {roleType.Precision} or more decimals than the scale {roleType.Scale} of {roleType}.");
            }

            roleType.AssertValue(unit);

            return unit;
        }

        /// <summary>
        /// Reads a unit from its XML value: from the text that <see cref="WriteString"/> writes for the unit and
        /// from no other text, and a float from a text in the form that <see cref="WriteString"/> writes, in which
        /// earlier .NET runtimes wrote other digits for the same float. <see cref="XmlConvert"/>,
        /// <see cref="Guid.Parse(string)"/> and <see cref="Convert.FromBase64String"/> read more than a save writes,
        /// such as white space around the text, 1 as a boolean and a year as a date.
        /// </summary>
        /// <param name="value">The XML value.</param>
        /// <param name="tag">The unit type tag.</param>
        /// <returns>The converted value.</returns>
        /// <exception cref="FormatException">
        /// The XML value is not the text that a save writes for a unit of the unit type.
        /// </exception>
        public static object ReadString(string value, string tag) =>
            tag switch
            {
                UnitTags.String => value != null ? ReadUtf8(value) : null,
                UnitTags.Integer => ReadInteger(value),
                UnitTags.Decimal => ReadDecimal(value),
                UnitTags.Float => ReadFloat(value),
                UnitTags.Boolean => ReadBoolean(value),
                UnitTags.DateTime => ReadDateTime(value),
                UnitTags.Unique => ReadUnique(value),
                UnitTags.Binary => ReadBase64(value, "a binary"),
                _ => throw new ArgumentException("Unknown Unit tag: " + tag)
            };

        /// <summary>
        /// <see cref="XmlConvert"/> the unit to an XML value..
        /// </summary>
        /// <param name="tag">The unit type tag.</param>
        /// <param name="unit">The unit .</param>
        /// <returns>The XML Value.</returns>
        public static string WriteString(string tag, object unit) =>
            tag switch
            {
                UnitTags.String => unit != null ? Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes((string)unit)) : null,
                UnitTags.Integer => XmlConvert.ToString((int)unit),
                UnitTags.Decimal => XmlConvert.ToString((decimal)unit),
                UnitTags.Float => XmlConvert.ToString((double)unit),
                UnitTags.Boolean => XmlConvert.ToString((bool)unit),
                UnitTags.DateTime => XmlConvert.ToString((DateTime)unit, XmlDateTimeSerializationMode.Utc),
                UnitTags.Unique => XmlConvert.ToString((Guid)unit),
                UnitTags.Binary => Convert.ToBase64String((byte[])unit),
                _ => throw new ArgumentException("Unknown Unit ObjectType: " + tag)
            };

        public static long EnsureVersion(long version)
        {
            var databaseInitial = (long)global::Allors.Version.DatabaseInitial;
            return version < databaseInitial ? databaseInitial : version;
        }

        // A save writes a string as the base64 of its UTF-8.
        private static string ReadUtf8(string value)
        {
            var bytes = ReadBase64(value, "a string");

            try
            {
                return Utf8.GetString(bytes);
            }
            catch (DecoderFallbackException e)
            {
                throw new FormatException($"The bytes of the text {Shown(value)} are not UTF-8: a save writes a string as the base64 of its UTF-8.", e);
            }
        }

        // A save writes bytes as Convert.ToBase64String does: four characters for every three bytes, without white
        // space, with = in place of the characters that the last bytes do not fill and with zeros in the bits that
        // they do not fill.
        private static byte[] ReadBase64(string value, string unit)
        {
            if (value != null && (value.Length & 3) == 0)
            {
                if (value.Length == 0)
                {
                    return Array.Empty<byte>();
                }

                var padding = value[value.Length - 1] != '=' ? 0 : value[value.Length - 2] != '=' ? 1 : 2;
                var bytes = new byte[(value.Length / 4 * 3) - padding];

                // The decoder skips white space, and then it gives fewer bytes than the text has characters for.
                if (Convert.TryFromBase64String(value, bytes, out var length) && length == bytes.Length && (padding == 0 || EndsAsSaved(value, bytes)))
                {
                    return bytes;
                }
            }

            throw NotSaved(value, unit, "base64 without white space, in four characters for every three bytes, with = in place of the characters that the last bytes do not fill");
        }

        // The last four characters of padded base64 hold one byte or two. The decoder takes any bits where a save
        // writes zeros: the bits that these bytes do not fill.
        private static bool EndsAsSaved(string value, byte[] bytes)
        {
            Span<char> end = stackalloc char[4];
            return Convert.TryToBase64Chars(bytes.AsSpan(bytes.Length - (bytes.Length % 3)), end, out _) && value.AsSpan(value.Length - 4).SequenceEqual(end);
        }

        private static int ReadInteger(string value)
        {
            // The text that a save writes is the only text of an integer: .NET also reads a plus sign, zeros before
            // the digits and a zero with a minus sign.
            Span<char> text = stackalloc char[11];
            if (int.TryParse(value, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out var integer) && IsSaved(value, integer, text))
            {
                return integer;
            }

            throw NotSaved(value, "an integer", "digits, with a minus sign before a negative integer and without a zero before the digits, from -2147483648 to 2147483647");
        }

        private static decimal ReadDecimal(string value)
        {
            // A decimal keeps the decimals of its text, zeros included, so that the text that a save writes is the
            // only text of a decimal: .NET also reads a plus sign, zeros before the digits, a zero with a minus sign
            // and a point without a digit before it or after it, and it rounds what has more decimals or more digits
            // than a decimal has.
            Span<char> text = stackalloc char[32];
            if (decimal.TryParse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out var @decimal) && IsSaved(value, @decimal, text))
            {
                return @decimal;
            }

            throw NotSaved(value, "a decimal", "digits and, for decimals, a point and digits, with a minus sign before a negative decimal and without a zero before the digits other than the one before the point, within the 28 decimals and the range of a decimal");
        }

        // Whether the text is the text that a save writes for an integer, a decimal or a unique, which XmlConvert
        // writes in the default format of the invariant culture.
        private static bool IsSaved<T>(string value, T unit, Span<char> text)
            where T : ISpanFormattable =>
            unit.TryFormat(text, out var length, default, NumberFormatInfo.InvariantInfo) && value.AsSpan().SequenceEqual(text.Slice(0, length));

        private static double ReadFloat(string value)
        {
            switch (value)
            {
                case "NaN":
                    return double.NaN;

                case "INF":
                    return double.PositiveInfinity;

                case "-INF":
                    return double.NegativeInfinity;
            }

            // A text beyond the largest float is not read as an infinity, and a text that is closer to zero than the
            // smallest float is not read as zero.
            if (IsFloat(value)
                && double.TryParse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out var @float)
                && double.IsFinite(@float)
                && (@float != 0 || value == "0" || value == "-0"))
            {
                return @float;
            }

            throw NotSaved(value, "a float", "digits and, for decimals, a point and digits that do not end in a zero, with a minus sign before a negative float and without a zero before the digits other than the one before the point, and for a large or a small float one digit that is not a zero, its decimals, and E, a sign and two or three digits; or NaN, INF or -INF");
        }

        // Whether the text has the form in which a save writes a float that is a number. A save writes a float in the
        // R format, for which the .NET Framework wrote other digits and another notation than .NET does:
        // 0.33333333333333331 and 1E+15 where .NET writes 0.3333333333333333 and 1000000000000000. So a float is read
        // by the form that both write, and not by the text that this .NET writes for it.
        private static bool IsFloat(ReadOnlySpan<char> text)
        {
            var index = 0;
            if (index < text.Length && text[index] == '-')
            {
                index++;
            }

            // The digits before the point: a zero, or digits that do not start with a zero.
            var start = index;
            while (index < text.Length && char.IsAsciiDigit(text[index]))
            {
                index++;
            }

            var digits = index - start;
            if (digits == 0 || (digits > 1 && text[start] == '0'))
            {
                return false;
            }

            // The decimals, which do not end in a zero.
            if (index < text.Length && text[index] == '.')
            {
                var decimals = ++index;
                while (index < text.Length && char.IsAsciiDigit(text[index]))
                {
                    index++;
                }

                if (index == decimals || text[index - 1] == '0')
                {
                    return false;
                }
            }

            if (index == text.Length)
            {
                return true;
            }

            // The exponent: E, a sign and two or three digits, after one digit that is not a zero and its decimals.
            if (text[index] != 'E' || digits != 1 || text[start] == '0')
            {
                return false;
            }

            index++;
            if (index == text.Length || (text[index] != '+' && text[index] != '-'))
            {
                return false;
            }

            var exponent = ++index;
            while (index < text.Length && char.IsAsciiDigit(text[index]))
            {
                index++;
            }

            var exponentDigits = index - exponent;
            return index == text.Length && (exponentDigits == 2 || (exponentDigits == 3 && text[exponent] != '0'));
        }

        private static bool ReadBoolean(string value) =>
            value switch
            {
                "true" => true,
                "false" => false,
                _ => throw NotSaved(value, "a boolean", "true or false"),
            };

        // A save writes a date in UTC as yyyy-MM-ddTHH:mm:ssZ, and one with a fraction of a second as
        // yyyy-MM-ddTHH:mm:ss.fffffffZ without the zeros at the end of the fraction. XmlConvert reads every date and
        // time of XML Schema: a date without a zone or with an offset, a day, a time, a month and a year.
        private static DateTime ReadDateTime(string value)
        {
            var text = value.AsSpan();
            if ((text.Length == 20 || (text.Length >= 22 && text.Length <= 28))
                && text[4] == '-' && text[7] == '-' && text[10] == 'T' && text[13] == ':' && text[16] == ':' && text[text.Length - 1] == 'Z'
                && ReadDigits(text.Slice(0, 4), out var year) && ReadDigits(text.Slice(5, 2), out var month) && ReadDigits(text.Slice(8, 2), out var day)
                && ReadDigits(text.Slice(11, 2), out var hour) && ReadDigits(text.Slice(14, 2), out var minute) && ReadDigits(text.Slice(17, 2), out var second)
                && year >= 1 && month >= 1 && month <= 12 && day >= 1 && day <= DateTime.DaysInMonth(year, month) && hour <= 23 && minute <= 59 && second <= 59)
            {
                var ticks = 0;
                if (text.Length == 20 || ReadFraction(text.Slice(19, text.Length - 20), out ticks))
                {
                    return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc).AddTicks(ticks);
                }
            }

            throw NotSaved(value, "a date", "yyyy-MM-ddTHH:mm:ssZ, a date in UTC, with a point and one to seven digits that do not end in a zero after the seconds of a date that has a fraction of a second");
        }

        // The fraction of a second in ticks, from a point and one to seven digits that do not end in a zero.
        private static bool ReadFraction(ReadOnlySpan<char> text, out int ticks)
        {
            if (text[0] != '.' || text[text.Length - 1] == '0' || !ReadDigits(text.Slice(1), out ticks))
            {
                ticks = 0;
                return false;
            }

            for (var digits = text.Length - 1; digits < 7; digits++)
            {
                ticks *= 10;
            }

            return true;
        }

        private static bool ReadDigits(ReadOnlySpan<char> text, out int number)
        {
            number = 0;
            foreach (var digit in text)
            {
                if (!char.IsAsciiDigit(digit))
                {
                    return false;
                }

                number = (number * 10) + (digit - '0');
            }

            return true;
        }

        private static Guid ReadUnique(string value)
        {
            // The text that a save writes is the only text of a unique: .NET also reads upper case, white space
            // around the text, and a sign or 0x in place of digits.
            Span<char> text = stackalloc char[36];
            if (Guid.TryParseExact(value, "D", out var unique) && IsSaved(value, unique, text))
            {
                return unique;
            }

            throw NotSaved(value, "a unique", "32 hexadecimal digits in lower case, in groups of 8, 4, 4, 4 and 12 with a hyphen between them");
        }

        // The exception of a reader for a text that a save does not write for a unit of its type.
        private static FormatException NotSaved(string value, string unit, string form) =>
            new FormatException($"The text {Shown(value)} is not {unit} as a save writes it: {form}.");

        // A text in the message of an exception. The text of another unit type can be long.
        private static string Shown(string value) =>
            value == null ? "null" : value.Length <= 64 ? "\"" + value + "\"" : "\"" + value.Substring(0, 64) + "...\" of " + value.Length.ToString(CultureInfo.InvariantCulture) + " characters";
    }
}
