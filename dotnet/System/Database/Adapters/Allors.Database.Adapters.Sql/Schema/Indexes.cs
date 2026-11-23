// <copyright file="Indexes.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using Meta;

    /// <summary>
    /// The indexes of the schema: the ones that the adapter wants, the step that brings the schema to them and
    /// the check that the schema has them.
    /// </summary>
    public abstract class Indexes
    {
        protected Indexes(Database database) => this.Database = database;

        protected Database Database { get; }

        protected virtual StringComparer IndexNameComparer => StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// The name of the lock that lets one database object at a time change the indexes of the schema.
        /// </summary>
        protected string LockName => "allors.indexes." + this.Database.SchemaName;

        /// <summary>
        /// Creates the indexes that the schema lacks and drops the ones of the adapter that it no longer wants.
        /// An index counts as present by its table and its leading column, whatever its name and whoever made
        /// it. An index is dropped only when it has the name that the adapter gives to an index for a unit role
        /// and that role gets no index.
        /// </summary>
        public void Ensure()
        {
            this.Derive(out var wanted, out var unwantedByName);

            var existing = this.Read();
            var missing = Missing(wanted, existing);
            var leftovers = existing
                .Where(v => unwantedByName.TryGetValue(v.Name, out var unwanted) && v.LeadsWithColumnOf(unwanted))
                .ToArray();

            if (missing.Length > 0)
            {
                this.RefuseATakenName(missing, existing);

                try
                {
                    this.Create(missing);
                }
                catch (DbException e)
                {
                    IReadOnlyList<SchemaIndex> remaining;
                    try
                    {
                        remaining = Missing(wanted, this.Read());
                    }
                    catch (DbException)
                    {
                        remaining = missing;
                    }

                    throw new Exception($"The schema lacks {remaining.Count} of its indexes and the adapter could not create them: {e.Message} Run these statements with a login that may create an index on the tables: {string.Join("; ", remaining.Select(this.CreateStatement))}", e);
                }
            }

            if (leftovers.Length > 0)
            {
                this.Drop(leftovers);
            }
        }

        /// <summary>
        /// Refuses a schema that lacks an index that the adapter wants. It reads the indexes of the schema and
        /// changes nothing: an index of the adapter that it no longer wants stays.
        /// </summary>
        public void Check()
        {
            this.Derive(out var wanted, out _);

            var existing = this.Read();
            var missing = Missing(wanted, existing);
            if (missing.Length > 0)
            {
                this.RefuseATakenName(missing, existing);

                throw new Exception($"The schema lacks {missing.Length} of its indexes, and a transaction creates none. Run the index command of the command-line tool or call EnsureIndexes() on the database, with a login that may create an index on the tables, or run these statements: {string.Join("; ", missing.Select(this.CreateStatement))}");
            }
        }

        /// <summary>
        /// Whether no complete index among the existing ones leads with the column of the index.
        /// </summary>
        protected static bool IsMissing(SchemaIndex index, IEnumerable<SchemaIndex> existing) => !existing.Any(v => v.IsComplete && v.LeadsWithColumnOf(index));

        protected string CreateStatement(SchemaIndex index) => $"CREATE INDEX {index.Name} ON {index.TableName} ({index.ColumnName})";

        /// <summary>
        /// Whether an index that exists prevents the creation of an index, because it has its name.
        /// </summary>
        protected abstract bool TakesNameOf(SchemaIndex existing, SchemaIndex index);

        /// <summary>
        /// Every index of the schema.
        /// </summary>
        protected abstract IReadOnlyList<SchemaIndex> Read();

        /// <summary>
        /// Creates the indexes that are still missing once the lock is held: another database object may have
        /// created them meanwhile.
        /// </summary>
        protected abstract void Create(IReadOnlyList<SchemaIndex> indexes);

        /// <summary>
        /// Drops the indexes. An index that can not be dropped now stays, and the next database object tries
        /// again.
        /// </summary>
        protected abstract void Drop(IReadOnlyList<SchemaIndex> indexes);

        // An index that is missing can not be created while another index has its name.
        private void RefuseATakenName(IReadOnlyList<SchemaIndex> missing, IReadOnlyList<SchemaIndex> existing)
        {
            foreach (var index in missing)
            {
                var taken = existing.FirstOrDefault(v => this.TakesNameOf(v, index));
                if (taken != null)
                {
                    throw new Exception($"The schema needs the index {index.Name} on {index.TableName} ({index.ColumnName}), and that name belongs to an index on {taken.TableName} ({taken.ColumnName ?? "an expression"}). Drop or rename that index.");
                }
            }
        }

        private static SchemaIndex[] Missing(IEnumerable<SchemaIndex> wanted, IReadOnlyList<SchemaIndex> existing)
        {
            var present = new HashSet<(string, string)>(existing.Where(v => v.IsComplete && v.ColumnName != null).Select(v => (v.TableName, v.ColumnName)));
            return wanted.Where(v => !present.Contains((v.TableName, v.ColumnName))).ToArray();
        }

        // An index for every composite relation, on its column in the table of a class or on the role column of
        // its own table, and for every unit role that the domain marks as indexed. A unit role that gets no index
        // is unwanted: an index with the name of the adapter on its column is a leftover.
        private void Derive(out List<SchemaIndex> wanted, out Dictionary<string, SchemaIndex> unwantedByName)
        {
            var mapping = this.Database.Mapping;
            wanted = new List<SchemaIndex>();
            unwantedByName = new Dictionary<string, SchemaIndex>(this.IndexNameComparer);

            foreach (var @class in this.Database.MetaPopulation.DatabaseClasses)
            {
                var tableName = mapping.TableNameForObjectByClass[@class];
                var className = @class.Name.ToLowerInvariant();

                foreach (var associationType in @class.DatabaseAssociationTypes)
                {
                    var relationType = associationType.RelationType;
                    var roleType = relationType.RoleType;
                    if (!(associationType.IsMany && roleType.IsMany) && relationType.ExistExclusiveDatabaseClasses && roleType.IsMany)
                    {
                        wanted.Add(new SchemaIndex($"idx_{className}_{associationType.SingularFullName.ToLowerInvariant()}", tableName, mapping.ColumnNameByRelationType[relationType]));
                    }
                }

                foreach (var roleType in @class.DatabaseRoleTypes)
                {
                    var relationType = roleType.RelationType;
                    var associationType = relationType.AssociationType;
                    var name = $"idx_{className}_{roleType.SingularFullName.ToLowerInvariant()}";

                    if (roleType.ObjectType.IsUnit)
                    {
                        var index = new SchemaIndex(name, tableName, mapping.ColumnNameByRelationType[relationType]);

                        // A string or a binary without a size, or with one above 4000, is too large for an index.
                        var unit = (IUnit)roleType.ObjectType;
                        var tooLarge = (unit.IsString || unit.IsBinary) && (roleType.Size == -1 || roleType.Size > 4000);
                        if (relationType.IsIndexed && !tooLarge)
                        {
                            wanted.Add(index);
                        }
                        else
                        {
                            unwantedByName[name] = index;
                        }
                    }
                    else if (!(associationType.IsMany && roleType.IsMany) && relationType.ExistExclusiveDatabaseClasses && !roleType.IsMany)
                    {
                        wanted.Add(new SchemaIndex(name, tableName, mapping.ColumnNameByRelationType[relationType]));
                    }
                }
            }

            foreach (var relationType in this.Database.MetaPopulation.DatabaseRelationTypes)
            {
                var associationType = relationType.AssociationType;
                var roleType = relationType.RoleType;
                if (!roleType.ObjectType.IsUnit && ((associationType.IsMany && roleType.IsMany) || !relationType.ExistExclusiveDatabaseClasses))
                {
                    wanted.Add(new SchemaIndex($"idx_{roleType.SingularFullName.ToLowerInvariant()}_{Mapping.ColumnNameForRole}", mapping.TableNameForRelationByRelationType[relationType], Mapping.ColumnNameForRole));
                }
            }
        }
    }
}
