// <copyright file="Import.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Xml;
    using Adapters;
    using Meta;
    using global::Npgsql;
    using NpgsqlTypes;

    public class Load
    {
        private readonly Database database;
        private readonly NpgsqlConnection connection;
        private readonly ObjectNotLoadedEventHandler objectNotLoaded;
        private readonly RelationNotLoadedEventHandler relationNotLoaded;

        private readonly Dictionary<long, IClass> classByObjectId;

        // The check on the roles of a relation type whose roles have one association. It is made when the first such
        // relation type is loaded, after the objects.
        private RolesWithOneAssociation rolesWithOneAssociation;

        public Load(Database database, NpgsqlConnection connection, ObjectNotLoadedEventHandler objectNotLoaded, RelationNotLoadedEventHandler relationNotLoaded)
        {
            this.database = database;
            this.connection = connection;
            this.objectNotLoaded = objectNotLoaded;
            this.relationNotLoaded = relationNotLoaded;

            this.classByObjectId = new Dictionary<long, IClass>();
        }

        public void Execute(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element) && reader.Name.Equals(Serialization.Population))
                {
                    var version = reader.GetAttribute(Serialization.Version);
                    if (string.IsNullOrEmpty(version))
                    {
                        throw new ArgumentException("Save population has no version.");
                    }

                    Serialization.CheckVersion(int.Parse(version));

                    if (!reader.IsEmptyElement)
                    {
                        this.LoadPopulation(reader);
                    }

                    break;
                }
            }
        }

        private void LoadPopulation(XmlReader reader)
        {
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    // eat everything but elements
                    case XmlNodeType.Element:
                        if (reader.Name.Equals(Serialization.Objects))
                        {
                            if (!reader.IsEmptyElement)
                            {
                                this.LoadObjects(reader.ReadSubtree());
                            }
                        }
                        else if (reader.Name.Equals(Serialization.Relations))
                        {
                            if (!reader.IsEmptyElement)
                            {
                                this.LoadRelations(reader.ReadSubtree());
                            }
                        }

                        break;
                }
            }
        }

        private void LoadObjects(XmlReader reader)
        {
            reader.MoveToContent();

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name.Equals(Serialization.Database))
                        {
                            if (!reader.IsEmptyElement)
                            {
                                this.LoadObjectsDatabase(reader.ReadSubtree());
                            }
                        }
                        else if (reader.Name.Equals(Serialization.Workspace))
                        {
                            throw new Exception("Can not load workspace objects in a database.");
                        }

                        break;
                }
            }
        }

        private void LoadObjectsDatabase(XmlReader reader)
        {
            var xmlObjects = new Objects(this.database, this.OnObjectNotLoaded, this.classByObjectId, reader);
            var mapping = this.database.Mapping;
            using (var writer = this.connection.BeginBinaryImport($"COPY {mapping.TableNameForObjects} ({Sql.Mapping.ColumnNameForObject}, {Sql.Mapping.ColumnNameForClass}, {Sql.Mapping.ColumnNameForVersion}) FROM STDIN (FORMAT BINARY)"))
            {
                foreach (var values in xmlObjects)
                {
                    writer.StartRow();
                    writer.Write(values[0], NpgsqlDbType.Bigint);
                    writer.Write(values[1], NpgsqlDbType.Uuid);
                    writer.Write(Serialization.EnsureVersion((long)values[2]), NpgsqlDbType.Bigint);
                }

                writer.Complete();
            }

            // The objects are copied with the ids of the file, which the sequence of the object ids does not follow.
            // Init started the sequence again, so move it to the highest loaded id: a new object gets the next id.
            // Without objects the sequence stays at its start.
            using (var command = this.connection.CreateCommand())
            {
                command.CommandTimeout = 0;
                command.CommandType = CommandType.Text;
                command.CommandText = $@"
select setval(pg_get_serial_sequence('{mapping.TableNameForObjects}', '{Sql.Mapping.ColumnNameForObject}')::regclass, max({Sql.Mapping.ColumnNameForObject}))
from {mapping.TableNameForObjects}
having max({Sql.Mapping.ColumnNameForObject}) is not null";

                command.ExecuteNonQuery();
            }

            // TODO: move this to a stored procedure
            // insert from _o table into class tables
            using (var transaction = this.connection.BeginTransaction())
            {
                foreach (var @class in this.database.MetaPopulation.DatabaseClasses)
                {
                    var tableName = this.database.Mapping.TableNameForObjectByClass[@class];

                    using (var command = this.connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandTimeout = 0;
                        command.CommandType = CommandType.Text;
                        command.CommandText = $@"
insert into {tableName} (o, c)
select o, c from {this.database.Mapping.TableNameForObjects}
where c = '{@class.Id}'";

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }

        private void LoadRelations(XmlReader reader)
        {
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name.Equals(Serialization.Database))
                        {
                            if (!reader.IsEmptyElement)
                            {
                                this.LoadRelationsDatabase(reader);
                            }
                        }
                        else if (reader.Name.Equals(Serialization.Workspace))
                        {
                            throw new Exception("Can not load workspace relations in a database.");
                        }

                        break;
                }
            }
        }

        private void LoadRelationsDatabase(XmlReader reader)
        {
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    // eat everything but elements
                    case XmlNodeType.Element:
                        if (!reader.IsEmptyElement)
                        {
                            if (reader.Name.Equals(Serialization.RelationTypeUnit)
                                || reader.Name.Equals(Serialization.RelationTypeComposite))
                            {
                                var relationTypeIdString = reader.GetAttribute(Serialization.Id);
                                if (string.IsNullOrEmpty(relationTypeIdString))
                                {
                                    throw new Exception("Relation type has no id");
                                }

                                var relationTypeId = new Guid(relationTypeIdString);
                                var relationType = (IRelationType)this.database.MetaPopulation.FindById(relationTypeId);

                                if (reader.Name.Equals(Serialization.RelationTypeUnit))
                                {
                                    if (relationType == null || relationType.RoleType.ObjectType is IComposite)
                                    {
                                        this.CantLoadUnitRole(reader.ReadSubtree(), relationTypeId);
                                    }
                                    else
                                    {
                                        this.LoadUnitRelations(reader.ReadSubtree(), relationType);
                                    }
                                }
                                else if (reader.Name.Equals(Serialization.RelationTypeComposite))
                                {
                                    if (relationType == null || relationType.RoleType.ObjectType is IUnit)
                                    {
                                        this.CantLoadCompositeRole(reader.ReadSubtree(), relationTypeId);
                                    }
                                    else
                                    {
                                        this.LoadCompositeRelations(reader.ReadSubtree(), relationType);
                                    }
                                }
                            }
                        }

                        break;
                }
            }
        }

        private void LoadUnitRelations(XmlReader reader, IRelationType relationType)
        {
            var allowedClasses = new HashSet<IClass>(relationType.AssociationType.ObjectType.DatabaseClasses);
            var unitRelationsByClass = new Dictionary<IClass, List<UnitRelation>>();

            var skip = false;
            while (skip || reader.Read())
            {
                skip = false;

                switch (reader.NodeType)
                {
                    // eat everything but elements
                    case XmlNodeType.Element:
                        if (reader.Name.Equals(Serialization.Relation))
                        {
                            var associationIdString = reader.GetAttribute(Serialization.Association);
                            var associationId = long.Parse(associationIdString);

                            this.classByObjectId.TryGetValue(associationId, out var @class);

                            if (@class == null || !allowedClasses.Contains(@class))
                            {
                                this.CantLoadUnitRole(reader.ReadSubtree(), relationType.Id);
                            }
                            else
                            {
                                if (!unitRelationsByClass.TryGetValue(@class, out var unitRelations))
                                {
                                    unitRelations = new List<UnitRelation>();
                                    unitRelationsByClass[@class] = unitRelations;
                                }

                                var isEmptyElement = reader.IsEmptyElement;
                                var value = string.Empty;
                                if (!isEmptyElement)
                                {
                                    value = reader.ReadElementContentAsString();
                                    skip = reader.IsStartElement();
                                }

                                try
                                {
                                    object unit = null;
                                    if (isEmptyElement)
                                    {
                                        var unitType = (IUnit)relationType.RoleType.ObjectType;
                                        switch (unitType.Tag)
                                        {
                                            case UnitTags.String:
                                                unit = string.Empty;
                                                break;

                                            case UnitTags.Binary:
                                                unit = Array.Empty<byte>();
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        unit = Serialization.ReadUnit(relationType.RoleType, value);
                                    }

                                    unitRelations.Add(new UnitRelation(associationId, unit));
                                }
                                catch
                                {
                                    this.OnRelationNotLoaded(relationType.Id, associationId, value);
                                }
                            }
                        }

                        break;
                }
            }

            var con = this.database.ConnectionFactory.Create();
            try
            {
                foreach (var kvp in unitRelationsByClass)
                {
                    var @class = kvp.Key;
                    var unitRelations = kvp.Value;

                    var sql = this.database.Mapping.ProcedureNameForSetUnitRoleByRelationTypeByClass[@class][relationType];
                    var command = con.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = CommandType.StoredProcedure;
                    command.UnitTableParameter(relationType.RoleType, unitRelations);
                    command.ExecuteNonQuery();
                }

                con.Commit();
            }
            catch
            {
                con.Rollback();
                throw;
            }
        }

        private void LoadCompositeRelations(XmlReader reader, IRelationType relationType)
        {
            var con = this.database.ConnectionFactory.Create();
            try
            {
                IEnumerable<CompositeRelation> relations = new CompositeRelations(
                    this.database,
                    relationType,
                    this.CantLoadCompositeRole,
                    this.OnRelationNotLoaded,
                    this.classByObjectId,
                    reader);

                // A role type whose roles have one association cannot hold a role twice.
                if (relationType.AssociationType.IsOne)
                {
                    this.rolesWithOneAssociation ??= new RolesWithOneAssociation(this.classByObjectId.Keys);
                    relations = this.rolesWithOneAssociation.Check(relationType, relations);
                }

                var sql = relationType.RoleType.IsOne ?
                              this.database.Mapping.ProcedureNameForSetRoleByRelationType[relationType] :
                              this.database.Mapping.ProcedureNameForAddRoleByRelationType[relationType];

                var command = con.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.StoredProcedure;
                command.AddCompositeRoleTableParameter(relations.ToArray());
                command.ExecuteNonQuery();

                con.Commit();
            }
            catch
            {
                con.Rollback();
                throw;
            }
        }

        private void CantLoadUnitRole(XmlReader reader, Guid relationTypeId)
        {
            var skip = false;
            while (skip || reader.Read())
            {
                skip = false;

                if (reader.IsStartElement() && reader.Name.Equals(Serialization.Relation))
                {
                    var a = reader.GetAttribute(Serialization.Association);
                    var value = string.Empty;

                    if (!reader.IsEmptyElement)
                    {
                        value = reader.ReadElementContentAsString();

                        // Reading the content moved the reader to the next record, which must not be read past.
                        skip = reader.IsStartElement();
                    }

                    this.OnRelationNotLoaded(relationTypeId, long.Parse(a), value);
                }
            }
        }

        private void CantLoadCompositeRole(XmlReader reader, Guid relationTypeId)
        {
            var skip = false;
            while (skip || reader.Read())
            {
                skip = false;

                if (reader.IsStartElement() && reader.Name.Equals(Serialization.Relation))
                {
                    var associationIdString = reader.GetAttribute(Serialization.Association);
                    var associationId = long.Parse(associationIdString);
                    if (string.IsNullOrEmpty(associationIdString))
                    {
                        throw new Exception("Association id is missing");
                    }

                    if (reader.IsEmptyElement)
                    {
                        this.OnRelationNotLoaded(relationTypeId, associationId, null);
                    }
                    else
                    {
                        var value = reader.ReadElementContentAsString();

                        // Reading the content moved the reader to the next record, which must not be read past.
                        skip = reader.IsStartElement();

                        foreach (var r in value.Split(Serialization.ObjectsSplitterCharArray))
                        {
                            this.OnRelationNotLoaded(relationTypeId, associationId, r);
                        }
                    }
                }
            }
        }

        #region Import Errors

        private void OnObjectNotLoaded(Guid objectTypeId, long allorsObjectId)
        {
            if (this.objectNotLoaded != null)
            {
                this.objectNotLoaded(this, new ObjectNotLoadedEventArgs(objectTypeId, allorsObjectId));
            }
            else
            {
                throw new Exception("Object not loaded: " + objectTypeId + ":" + allorsObjectId);
            }
        }

        private void OnRelationNotLoaded(Guid relationTypeId, long associationObjectId, string roleContents)
        {
            var args = new RelationNotLoadedEventArgs(relationTypeId, associationObjectId, roleContents);
            if (this.relationNotLoaded != null)
            {
                this.relationNotLoaded(this, args);
            }
            else
            {
                throw new Exception("Role not loaded: " + args);
            }
        }

        #endregion Import Errors
    }
}
