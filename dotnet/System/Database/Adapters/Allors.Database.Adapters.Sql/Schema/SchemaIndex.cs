// <copyright file="SchemaIndex.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;

    /// <summary>
    /// An index of the schema: its name, its table and the column that it leads with. The table and the column are
    /// named as the mapping names them.
    /// </summary>
    public sealed class SchemaIndex
    {
        public SchemaIndex(string name, string tableName, string columnName, bool isComplete = true)
        {
            this.Name = name;
            this.TableName = tableName;
            this.ColumnName = columnName;
            this.IsComplete = isComplete;
        }

        public string Name { get; }

        public string TableName { get; }

        /// <summary>
        /// The column that the index leads with. Null for an index that leads with an expression.
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// Whether the index covers every row and can be used. An index that is partial, disabled or not valid is
        /// not complete, and does not count as present.
        /// </summary>
        public bool IsComplete { get; }

        public bool HasNameOf(SchemaIndex other) => string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);

        public bool IsOnTableOf(SchemaIndex other) => string.Equals(this.TableName, other.TableName, StringComparison.Ordinal);

        public bool LeadsWithColumnOf(SchemaIndex other) => this.IsOnTableOf(other) && string.Equals(this.ColumnName, other.ColumnName, StringComparison.Ordinal);
    }
}
