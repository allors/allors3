// <copyright file="Load2.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Microsoft.Data.SqlClient;
    using System.Xml;

    using Adapters;

    using Meta;

    public class Load
    {
        private readonly Database database;
        private readonly SqlConnection connection;
        private readonly ObjectNotLoadedEventHandler objectNotLoaded;
        private readonly RelationNotLoadedEventHandler relationNotLoaded;

        private readonly Dictionary<long, IClass> classByObjectId;

        // The ids of the objects that were reported as not loaded. Their relations are reported as not loaded as
        // well. A relation of any other object that is not loaded stops the load: the file does not hold it.
        private readonly HashSet<long> objectsNotLoaded;

        public Load(Database database, SqlConnection connection, ObjectNotLoadedEventHandler objectNotLoaded, RelationNotLoadedEventHandler relationNotLoaded)
        {
            this.database = database;
            this.connection = connection;
            this.objectNotLoaded = objectNotLoaded;
            this.relationNotLoaded = relationNotLoaded;

            this.classByObjectId = new Dictionary<long, IClass>();
            this.objectsNotLoaded = new HashSet<long>();
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

                    return;
                }
            }

            throw Serialization.NoPopulation();
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
                        else
                        {
                            throw new Exception("Unknown child element <" + reader.Name + "> in parent element <" + Serialization.Population + ">");
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
                        else
                        {
                            throw new Exception("Unknown child element <" + reader.Name + "> in parent element <" + Serialization.Objects + ">");
                        }

                        break;
                }
            }
        }

        private void LoadObjectsDatabase(XmlReader reader)
        {
            var xmlObjects = new Objects(this.database, this.OnObjectNotLoaded, this.classByObjectId, reader);
            using (var objectsReader = new LoadObjectsReader(xmlObjects))
            using (var sqlBulkCopy = new SqlBulkCopy(this.connection, SqlBulkCopyOptions.KeepIdentity, null))
            {
                sqlBulkCopy.BulkCopyTimeout = 0;
                sqlBulkCopy.BatchSize = 5000;
                sqlBulkCopy.DestinationTableName = this.database.Mapping.TableNameForObjects;
                sqlBulkCopy.WriteToServer(objectsReader);
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
                                this.LoadRelationsDatabase(reader);
                            }
                        }
                        else if (reader.Name.Equals(Serialization.Workspace))
                        {
                            throw new Exception("Can not load workspace relations in a database.");
                        }
                        else
                        {
                            throw new Exception("Unknown child element <" + reader.Name + "> in parent element <" + Serialization.Relations + ">");
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
                        if (reader.Name.Equals(Serialization.RelationTypeUnit)
                            || reader.Name.Equals(Serialization.RelationTypeComposite))
                        {
                            // A self-closing element holds no relations.
                            if (!reader.IsEmptyElement)
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
                        else
                        {
                            throw new Exception("Unknown child element <" + reader.Name + "> in parent element <" + Serialization.Database + ">");
                        }

                        break;

                    case XmlNodeType.EndElement:
                        // The end of the database element. What follows it belongs to the relations element.
                        return;
                }
            }
        }

        private void LoadUnitRelations(XmlReader reader, IRelationType relationType)
        {
            var allowedClasses = new HashSet<IClass>(relationType.AssociationType.ObjectType.DatabaseClasses);
            var unitRelationsByClass = new Dictionary<IClass, List<UnitRelation>>();

            // The reader holds the relation type element and what is in it: move to the element, so that the loop
            // reads its children.
            reader.MoveToContent();

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

                            // An object that is not loaded was reported as not loaded, and then its relations are
                            // reported as well, or the file does not hold it, which stops the load.
                            if (@class == null && !this.objectsNotLoaded.Contains(associationId))
                            {
                                throw Serialization.NoObject(relationType, associationId, associationId);
                            }

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
                                        unit = unitType.Tag switch
                                        {
                                            UnitTags.String => string.Empty,
                                            UnitTags.Binary => Array.Empty<byte>(),
                                            _ => unit
                                        };
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
                        else
                        {
                            throw new Exception("Unknown child element <" + reader.Name + "> in parent element <" + Serialization.RelationTypeUnit + ">");
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

                    // Every relation of the class was reported as not loaded. SQL Server does not accept a
                    // table-valued parameter without rows.
                    if (unitRelations.Count == 0)
                    {
                        continue;
                    }

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
                var relations = new LoadCompositeRelations(
                    this.database,
                    relationType,
                    this.CantLoadCompositeRole,
                    this.OnRelationNotLoaded,
                    this.classByObjectId,
                    this.objectsNotLoaded,
                    reader);

                var sql = relationType.RoleType.IsOne ?
                              this.database.Mapping.ProcedureNameForSetRoleByRelationType[relationType] :
                              this.database.Mapping.ProcedureNameForAddRoleByRelationType[relationType];

                // The relations are read while they are written. SQL Server does not accept a table-valued
                // parameter without rows, so nothing is written for a relation type without a relation to load:
                // it has none, or every one of them was reported as not loaded.
                using (var enumerator = relations.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        var command = con.CreateCommand();
                        command.CommandText = sql;
                        command.CommandType = CommandType.StoredProcedure;
                        command.AddCompositeRoleTableParameter(Remaining(enumerator));
                        command.ExecuteNonQuery();
                    }
                }

                con.Commit();
            }
            catch
            {
                con.Rollback();
                throw;
            }
        }

        // The relation that the enumerator stands on and the relations after it.
        private static IEnumerable<CompositeRelation> Remaining(IEnumerator<CompositeRelation> enumerator)
        {
            do
            {
                yield return enumerator.Current;
            }
            while (enumerator.MoveNext());
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
                this.objectsNotLoaded.Add(allorsObjectId);
            }
            else
            {
                throw new Exception($"Object not loaded: {objectTypeId}:{allorsObjectId}");
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
                throw new Exception($"Role not loaded: {args}");
            }
        }

        #endregion Import Errors
    }
}
