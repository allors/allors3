// <copyright file="Serialization.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Serialization type.</summary>

namespace Allors.Database.Adapters
{
    using System;
    using System.Xml;

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
        /// <remarks>
        /// Version 1 is ambiguous, see <see cref="LoadOptions.Version1StringEncoding"/>.
        /// Version 2 always stores string unit roles Base64 encoded.
        /// </remarks>
        public const int VersionCurrent = 2;

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
        /// Resolves the <see cref="StringEncoding"/> of the string unit roles of a population with the
        /// given <see cref="Serialization#Version"/>, and rejects versions that are not supported.
        /// </summary>
        /// <param name="version">The serialization version of the population.</param>
        /// <param name="options">The load options, can be null.</param>
        /// <returns>The encoding of the string unit roles.</returns>
        public static StringEncoding ResolveStringEncoding(int version, LoadOptions options) =>
            version switch
            {
                VersionCurrent => StringEncoding.Base64,
                1 => options?.Version1StringEncoding ?? throw new ArgumentException(
                        "Version 1 populations are ambiguous: those saved before 2023-07-05 store string unit roles as raw xml text, later ones store them Base64 encoded. " +
                        "Set LoadOptions.Version1StringEncoding (command line: --v1-strings Raw|Base64) to state which one this is. " +
                        "Saving the population again upgrades it to version " + VersionCurrent + "."),
                _ => throw new ArgumentException("Database supports versions 1 and " + VersionCurrent + " but found version " + version + ".")
            };

        /// <summary>
        /// <see cref="XmlConvert"/> from the xml unit value.
        /// </summary>
        /// <param name="value">The XML value.</param>
        /// <param name="tag">The unit type tag.</param>
        /// <param name="stringEncoding">The encoding of the string unit roles.</param>
        /// <returns>The converted value.</returns>
        public static object ReadString(string value, string tag, StringEncoding stringEncoding) =>
            tag switch
            {
                UnitTags.String => ReadStringUnit(value, stringEncoding),
                UnitTags.Integer => XmlConvert.ToInt32(value),
                UnitTags.Decimal => XmlConvert.ToDecimal(value),
                UnitTags.Float => XmlConvert.ToDouble(value),
                UnitTags.Boolean => XmlConvert.ToBoolean(value),
                UnitTags.DateTime => XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Utc),
                UnitTags.Unique => Guid.Parse(value),
                UnitTags.Binary => Convert.FromBase64String(value),
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

        private static string ReadStringUnit(string value, StringEncoding stringEncoding)
        {
            if (value == null)
            {
                return null;
            }

            return stringEncoding switch
            {
                StringEncoding.Base64 => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value)),
                StringEncoding.Raw => value,
                _ => throw new ArgumentException("Unknown StringEncoding: " + stringEncoding)
            };
        }

        public static long EnsureVersion(long version)
        {
            var databaseInitial = (long)global::Allors.Version.DatabaseInitial;
            return version < databaseInitial ? databaseInitial : version;
        }
    }
}
