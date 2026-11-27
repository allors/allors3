// <copyright file="Validation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Meta;

    public abstract class Validation
    {
        public abstract string Message { get; }

        public abstract bool IsValid { get; }

        // A single role value is keyed by its association. A collection is keyed by the pair (association, role).
        protected static bool HasRelationPrimaryKey(IRoleType roleType, int keyColumnCount, int? associationOrdinal, int? roleOrdinal) =>
            associationOrdinal == 1 && (roleType.IsOne ? keyColumnCount == 1 : keyColumnCount == 2 && roleOrdinal == 2);

        /// <summary>
        /// The lines of a message for the columns of a table: each column that the table lacks or has with another
        /// type, as the model has it, and each column that the model lacks, as its name says. No line when the
        /// columns are those of the model.
        /// </summary>
        protected static List<string> CompareColumns(IEnumerable<(string Name, string SqlType, string Description)> expected, IReadOnlyDictionary<string, string> sqlTypeByColumn, Func<string, string> describeObsolete)
        {
            var lines = new List<string>();
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var (name, sqlType, description) in expected)
            {
                names.Add(name);
                if (!sqlTypeByColumn.TryGetValue(name, out var actual))
                {
                    lines.Add($"{name} is missing: {description}");
                }
                else if (!string.Equals(actual, sqlType, StringComparison.Ordinal))
                {
                    lines.Add($"{name} has the type {actual} and needs {sqlType}: {description}");
                }
            }

            foreach (var name in sqlTypeByColumn.Keys.Where(v => !names.Contains(v)).OrderBy(v => v, StringComparer.Ordinal))
            {
                lines.Add($"{name} is obsolete: {describeObsolete(name)}");
            }

            return lines;
        }

        // The line for the primary key of the table of a relation type that does not have the key of its role: the
        // columns of the key in their order, and those of the key that the role needs.
        protected static string PrimaryKeyLine(IRoleType roleType, IEnumerable<(string Name, int Ordinal)> keyColumns, string associationColumn, string roleColumn)
        {
            var key = keyColumns.Where(v => v.Ordinal > 0).OrderBy(v => v.Ordinal).Select(v => v.Name).ToArray();
            var needed = roleType.IsOne ? associationColumn : associationColumn + ", " + roleColumn;
            return key.Length == 0
                ? $"the primary key is missing and needs to be on ({needed})"
                : $"the primary key is on ({string.Join(", ", key)}) and needs to be on ({needed})";
        }

        // A heading, and a line for each table with the lines of its columns under it.
        protected static void Append(StringBuilder message, string heading, IEnumerable<(string Name, string Description, IEnumerable<string> Lines)> tables)
        {
            var empty = true;
            foreach (var (name, description, lines) in tables)
            {
                if (empty)
                {
                    message.Append(heading).Append(":\n");
                    empty = false;
                }

                message.Append("- ").Append(name).Append(": ").Append(description).Append('\n');
                foreach (var line in lines)
                {
                    message.Append("  - ").Append(line).Append('\n');
                }
            }
        }

        protected static string DescribeObjectsTableColumn(string column) => column switch
        {
            Mapping.ColumnNameForObject => "the id of the object",
            Mapping.ColumnNameForClass => "the class of the object",
            Mapping.ColumnNameForVersion => "the version of the object",
            _ => "a column that is not of the model",
        };

        protected static string DescribeRole(IRelationType relationType) =>
            relationType.RoleType.ObjectType.IsUnit
                ? "the role of the relation type " + Mapping.Name(relationType)
                : "the role of the relation type " + Mapping.Name(relationType) + ", " + relationType.Multiplicity + ", whose object type is " + relationType.RoleType.ObjectType.Name;

        protected static string DescribeAssociation(IRelationType relationType) =>
            "the association of the relation type " + Mapping.Name(relationType) + ", " + relationType.Multiplicity + ", whose object type is " + relationType.AssociationType.ObjectType.Name;

        protected static string DescribeRelationTableAssociation(IRelationType relationType) => "the association, whose object type is " + relationType.AssociationType.ObjectType.Name;

        protected static string DescribeRelationTableRole(IRelationType relationType) => "the role, whose object type is " + relationType.RoleType.ObjectType.Name;

        /// <summary>
        /// A table of the schema whose name starts with the prefix and that the model lacks, as its name says: the
        /// table of a class, or the table of a relation type with its multiplicity. A class or a relation type that
        /// the model lacks shows its id.
        /// </summary>
        protected static string DescribeObsoleteTable(IMetaPopulation metaPopulation, string tableName)
        {
            var name = tableName.AsSpan(Mapping.Prefix.Length);
            if (ModelId.Decode(name) is { } classId)
            {
                return "the table of the class " + NameOf(metaPopulation, classId);
            }

            if (name.Length == 29 && name[26] == '_' && ModelId.Decode(name[..26]) is { } relationTypeId && MultiplicityOf(name[27..]) is { } multiplicity)
            {
                return "the table of the relation type " + NameOf(metaPopulation, relationTypeId) + ", " + multiplicity;
            }

            return "a table that is not of the model";
        }

        /// <summary>
        /// A column of the table of a class that the model lacks, as its name says: a unit role, or a composite role
        /// or association with its multiplicity and its object type.
        /// </summary>
        protected static string DescribeObsoleteClassTableColumn(IMetaPopulation metaPopulation, string columnName)
        {
            var parts = columnName.Split('_');
            if (parts.Length == 2 && parts[0] == "r" && ModelId.Decode(parts[1]) is { } unitRelationTypeId)
            {
                return "the role of the relation type " + NameOf(metaPopulation, unitRelationTypeId);
            }

            if (parts.Length == 4 && (parts[0] is "a" or "r") && ModelId.Decode(parts[1]) is { } relationTypeId && ModelId.Decode(parts[2]) is { } objectTypeId && MultiplicityOf(parts[3]) is { } multiplicity)
            {
                return (parts[0] == "a" ? "the association" : "the role") + " of the relation type " + NameOf(metaPopulation, relationTypeId) + ", " + multiplicity + ", whose object type is " + NameOf(metaPopulation, objectTypeId);
            }

            return "a column that is not of the model";
        }

        /// <summary>
        /// A column of the table of a relation type that the model lacks, as its name says: the association or the
        /// role, with its object type.
        /// </summary>
        protected static string DescribeObsoleteRelationTableColumn(IMetaPopulation metaPopulation, string columnName)
        {
            var parts = columnName.Split('_');
            if (parts.Length == 2 && (parts[0] is "a" or "r") && ModelId.Decode(parts[1]) is { } objectTypeId)
            {
                return (parts[0] == "a" ? "the association" : "the role") + ", whose object type is " + NameOf(metaPopulation, objectTypeId);
            }

            return "a column that is not of the model";
        }

        // The name of the object type or of the relation type of the model with the id, or the id.
        private static string NameOf(IMetaPopulation metaPopulation, Guid id) =>
            metaPopulation.FindById(id) switch
            {
                IRelationType relationType => Mapping.Name(relationType),
                IObjectType objectType => objectType.Name,
                _ => id.ToString(),
            };

        private static Multiplicity? MultiplicityOf(ReadOnlySpan<char> tag) => tag switch
        {
            "oo" => Multiplicity.OneToOne,
            "om" => Multiplicity.OneToMany,
            "mo" => Multiplicity.ManyToOne,
            "mm" => Multiplicity.ManyToMany,
            _ => null,
        };
    }
}
