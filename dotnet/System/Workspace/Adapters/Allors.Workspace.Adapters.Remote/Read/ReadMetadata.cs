// <copyright file="ReadMetadata.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml;
    using System.Text.RegularExpressions;
    using Allors.Protocol.Json.Api.Sync;
    using Meta;

    /// <summary>Owned metadata projection; later changes to caller metadata cannot change an operation.</summary>
    internal sealed class ReadMetadata
    {
        private readonly Dictionary<string, Dictionary<string, Role>> classes;
        private readonly Dictionary<string, HashSet<string>> methods;

        internal ReadMetadata(IMetaPopulation population)
        {
            if (population == null) { throw new ArgumentNullException(nameof(population)); }
            this.classes = population.Composites.OfType<IClass>().ToDictionary(v => v.Tag,
                v => v.DatabaseOriginRoleTypes.ToDictionary(r => r.RelationType.Tag, r => new Role(r.ObjectType.IsUnit ? r.ObjectType.Tag : null, r.IsMany, r.ObjectType is IComposite composite ? composite.Classes.Select(c => c.Tag) : Array.Empty<string>())));
            this.methods = population.Composites.OfType<IClass>().ToDictionary(v => v.Tag, v => new HashSet<string>(v.MethodTypes.Select(m => m.Tag)));
        }

        internal void Class(string classTag)
        {
            if (!this.classes.ContainsKey(classTag)) { throw new FormatException("The read class is absent from configured workspace metadata. Use the matching representation and metadata."); }
        }

        internal IReadOnlyDictionary<string, object> Roles(string classTag, SyncResponseRole[] roles)
        {
            this.Class(classTag);
            var result = new Dictionary<string, object>();
            foreach (var value in roles)
            {
                if (!this.classes[classTag].TryGetValue(value.t, out var role)) { throw new FormatException("The read role does not belong to this class in the configured workspace metadata."); }
                object decoded;
                if (role.Unit != null)
                {
                    if (value.o != null || value.c != null) { throw new FormatException("Encode a unit role with its value field, not a relation ID or collection."); }
                    decoded = Unit(role.Unit, value.v);
                }
                else if (role.Many)
                {
                    if (value.o != null || value.v != null || value.c == null) { throw new FormatException("Encode a to-many role with an explicit ID collection."); }
                    decoded = value.c.ToArray();
                }
                else
                {
                    if (value.c != null || value.v != null) { throw new FormatException("Encode a to-one role with one object ID or null."); }
                    decoded = value.o;
                }

                result.Add(value.t, decoded);
            }

            return result;
        }

        internal void Record(ReadStoredRecord record)
        {
            this.Class(record.ClassTag);
            foreach (var value in record.Roles)
            {
                if (!this.classes[record.ClassTag].TryGetValue(value.Key, out var role)) { throw new FormatException("The retained role is absent from the configured class metadata."); }
                if (role.Unit != null)
                {
                    var valid = value.Value == null || (role.Unit == UnitTags.Binary && value.Value is byte[]) ||
                        (role.Unit == UnitTags.Boolean && value.Value is bool) || (role.Unit == UnitTags.DateTime && value.Value is DateTime) ||
                        (role.Unit == UnitTags.Decimal && value.Value is decimal) || (role.Unit == UnitTags.Float && value.Value is double number && !double.IsNaN(number) && !double.IsInfinity(number)) ||
                        (role.Unit == UnitTags.Integer && value.Value is int) || (role.Unit == UnitTags.String && value.Value is string) || (role.Unit == UnitTags.Unique && value.Value is Guid);
                    if (!valid) { throw new FormatException("The retained unit does not match configured metadata; retain a correctly decoded body."); }
                }
                else if (role.Many ? !(value.Value is long[] ids) || ids.Any(v => v <= 0 || v > 9007199254740991) || !ids.SequenceEqual(ids.Distinct().OrderBy(v => v)) : value.Value != null && (!(value.Value is long id) || id <= 0 || id > 9007199254740991))
                {
                    throw new FormatException("The retained relation must contain positive interoperable database IDs with the configured cardinality.");
                }
            }
        }

        internal void Pool(ReadStoredRecord[] records)
        {
            var byId = records.ToDictionary(v => v.Id);
            foreach (var record in records)
            {
                foreach (var value in record.Roles)
                {
                    var role = this.classes[record.ClassTag][value.Key];
                    if (role.Unit != null || value.Value == null) { continue; }
                    var targets = role.Many ? (long[])value.Value : new[] { (long)value.Value };
                    if (targets.Any(id => byId.TryGetValue(id, out var target) && !role.Targets.Contains(target.ClassTag)))
                    { throw new FormatException("A loaded relation target has a class incompatible with the configured role metadata."); }
                }
            }
        }

        internal void Permission(ReadStoredPermission permission)
        {
            this.Class(permission.ClassTag);
            if (permission.Operation == Operations.Create) { return; }
            if (permission.Operation == Operations.Execute ? !this.methods[permission.ClassTag].Contains(permission.OperandTag) : !this.classes[permission.ClassTag].ContainsKey(permission.OperandTag))
            { throw new FormatException("The permission operand does not belong to its configured class or operation kind."); }
        }

        private static object Unit(string tag, object value)
        {
            if (value == null) { return null; }
            try
            {
                if (tag == UnitTags.String && value is string text) { return text; }
                if (tag == UnitTags.Boolean && value is bool boolean) { return boolean; }
                if (tag == UnitTags.Binary && value is string binary && !binary.Any(char.IsWhiteSpace)) { return Convert.FromBase64String(binary); }
                if (tag == UnitTags.DateTime && value is string date &&
                    Regex.IsMatch(date, @"^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}(?:\.[0-9]{1,7})?(?:Z|[+-][0-9]{2}:[0-9]{2})$", RegexOptions.CultureInvariant) &&
                    int.Parse(date.Substring(11, 2), CultureInfo.InvariantCulture) < 24)
                { return XmlConvert.ToDateTimeOffset(date).UtcDateTime; }
                if (tag == UnitTags.Unique && value is string unique && Guid.TryParseExact(unique, "D", out var guid)) { return guid; }
                if (tag == UnitTags.Decimal && value is string decimalText && Regex.IsMatch(decimalText, @"^-?[0-9]+(?:\.[0-9]+)?$", RegexOptions.CultureInvariant))
                {
                    var parts = decimalText.TrimStart('-').Split('.');
                    var coefficient = string.Concat(parts).TrimStart('0');
                    const string maximum = "79228162514264337593543950335";
                    if ((parts.Length == 1 || parts[1].Length <= 28) &&
                        (coefficient.Length < maximum.Length || (coefficient.Length == maximum.Length && string.CompareOrdinal(coefficient, maximum) <= 0)))
                    { return decimal.Parse(decimalText, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture); }
                }
                if (tag == UnitTags.Integer && IsNumber(value))
                {
                    var number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                    if (number >= int.MinValue && number <= int.MaxValue && Math.Truncate(number) == number) { return (int)number; }
                }

                if (tag == UnitTags.Float && IsNumber(value))
                {
                    var number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                    if (!double.IsNaN(number) && !double.IsInfinity(number)) { return number; }
                }
            }
            catch (Exception exception) when (exception is FormatException || exception is OverflowException || exception is ArgumentException)
            { throw new FormatException("The read unit encoding does not match the configured workspace metadata.", exception); }
            throw new FormatException("The read unit encoding does not match the configured workspace metadata.");
        }

        private static bool IsNumber(object value) => value is long || value is int || value is double || value is float || value is decimal;
        private sealed class Role
        {
            internal Role(string unit, bool many, IEnumerable<string> targets) { this.Unit = unit; this.Many = many; this.Targets = new HashSet<string>(targets); }
            internal HashSet<string> Targets { get; }
            internal string Unit { get; }
            internal bool Many { get; }
        }
    }
}
