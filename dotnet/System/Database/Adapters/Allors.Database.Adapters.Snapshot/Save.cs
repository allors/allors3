// <copyright file="Save.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Snapshot
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml;
    using Adapters;
    using Meta;

    /// <summary>
    /// Writes the committed state of a snapshot as an XML population, independently of any transaction.
    /// </summary>
    internal sealed class Save
    {
        private readonly Database database;
        private readonly XmlWriter writer;
        private readonly Snapshot snapshot;

        internal Save(Database database, XmlWriter writer, Snapshot snapshot)
        {
            this.database = database;
            this.writer = writer;
            this.snapshot = snapshot;
        }

        internal void Execute()
        {
            var writeDocument = false;
            if (this.writer.WriteState == WriteState.Start)
            {
                this.writer.WriteStartDocument();
                this.writer.WriteStartElement(Serialization.Allors);
                writeDocument = true;
            }

            this.SavePopulation();

            if (writeDocument)
            {
                this.writer.WriteEndElement();
                this.writer.WriteEndDocument();
            }
        }

        private static string Format(long value) => value.ToString(CultureInfo.InvariantCulture);

        private void SavePopulation()
        {
            this.writer.WriteStartElement(Serialization.Population);
            this.writer.WriteAttributeString(Serialization.Version, Serialization.VersionCurrent.ToString(CultureInfo.InvariantCulture));

            this.SaveObjectTypes();

            this.SaveRelationTypes();

            this.writer.WriteEndElement();
        }

        private void SaveObjectTypes()
        {
            this.writer.WriteStartElement(Serialization.Objects);
            this.writer.WriteStartElement(Serialization.Database);

            var sortedObjectTypes = new List<IObjectType>(this.snapshot.ObjectIdsByClass.Keys);
            sortedObjectTypes.Sort();

            foreach (var objectType in sortedObjectTypes)
            {
                var objectIds = this.snapshot.ObjectIdsByClass[(IClass)objectType];
                if (objectIds.IsEmpty)
                {
                    continue;
                }

                this.writer.WriteStartElement(Serialization.ObjectType);
                this.writer.WriteAttributeString(Serialization.Id, objectType.Id.ToString("N").ToLowerInvariant());

                var first = true;
                foreach (var objectId in objectIds)
                {
                    if (!first)
                    {
                        this.writer.WriteString(Serialization.ObjectsSplitter);
                    }

                    first = false;

                    var record = this.snapshot.RecordById[objectId];
                    this.writer.WriteString(Format(record.ObjectId) + Serialization.ObjectSplitter + Format(record.Version));
                }

                this.writer.WriteEndElement();
            }

            this.writer.WriteEndElement();
            this.writer.WriteEndElement();
        }

        private void SaveRelationTypes()
        {
            this.writer.WriteStartElement(Serialization.Relations);
            this.writer.WriteStartElement(Serialization.Database);

            // When two transactions change the same object the last one to commit wins, so a relation may
            // point to an object that another transaction deleted. Such a relation is not part of the
            // population and is not written.
            var recordsByRoleType = new Dictionary<IRoleType, List<Record>>();
            foreach (var kvp in this.snapshot.RecordById)
            {
                var record = kvp.Value;

                foreach (var unitRole in record.UnitRoles())
                {
                    Add(recordsByRoleType, unitRole.Key, record);
                }

                foreach (var compositeRole in record.CompositeRoles())
                {
                    if (this.Exists(compositeRole.Value))
                    {
                        Add(recordsByRoleType, compositeRole.Key, record);
                    }
                }

                foreach (var compositesRole in record.CompositesRoles())
                {
                    if (compositesRole.Value.Any(this.Exists))
                    {
                        Add(recordsByRoleType, compositesRole.Key, record);
                    }
                }
            }

            foreach (var records in recordsByRoleType.Values)
            {
                records.Sort((x, y) => x.ObjectId.CompareTo(y.ObjectId));
            }

            var sortedRelationTypes = new List<IRelationType>(this.database.MetaPopulation.DatabaseRelationTypes);
            sortedRelationTypes.Sort();
            foreach (var relationType in sortedRelationTypes)
            {
                var roleType = relationType.RoleType;

                if (!recordsByRoleType.TryGetValue(roleType, out var records))
                {
                    continue;
                }

                var unitType = roleType.ObjectType as IUnit;

                this.writer.WriteStartElement(unitType != null
                                             ? Serialization.RelationTypeUnit
                                             : Serialization.RelationTypeComposite);

                this.writer.WriteAttributeString(Serialization.Id, relationType.Id.ToString("N").ToLowerInvariant());

                if (unitType != null)
                {
                    foreach (var record in records)
                    {
                        this.SaveUnit(record, roleType, unitType);
                    }
                }
                else if (roleType.IsMany)
                {
                    foreach (var record in records)
                    {
                        this.SaveComposites(record, roleType);
                    }
                }
                else
                {
                    foreach (var record in records)
                    {
                        this.SaveComposite(record, roleType);
                    }
                }

                this.writer.WriteEndElement();
            }

            this.writer.WriteEndElement();
            this.writer.WriteEndElement();
        }

        private static void Add(Dictionary<IRoleType, List<Record>> recordsByRoleType, IRoleType roleType, Record record)
        {
            if (!recordsByRoleType.TryGetValue(roleType, out var records))
            {
                records = new List<Record>();
                recordsByRoleType.Add(roleType, records);
            }

            records.Add(record);
        }

        private void SaveUnit(Record record, IRoleType roleType, IUnit unitType)
        {
            var value = Serialization.WriteString(unitType.Tag, record.GetUnitRole(roleType));

            this.writer.WriteStartElement(Serialization.Relation);
            this.writer.WriteAttributeString(Serialization.Association, Format(record.ObjectId));
            this.writer.WriteString(value);
            this.writer.WriteEndElement();
        }

        private void SaveComposites(Record record, IRoleType roleType)
        {
            this.writer.WriteStartElement(Serialization.Relation);
            this.writer.WriteAttributeString(Serialization.Association, Format(record.ObjectId));

            var first = true;
            foreach (var roleId in record.GetCompositesRole(roleType))
            {
                if (!this.Exists(roleId))
                {
                    continue;
                }

                if (!first)
                {
                    this.writer.WriteString(Serialization.ObjectsSplitter);
                }

                first = false;

                this.writer.WriteString(Format(roleId));
            }

            this.writer.WriteEndElement();
        }

        private bool Exists(long objectId) => this.snapshot.RecordById.ContainsKey(objectId);

        private void SaveComposite(Record record, IRoleType roleType)
        {
            this.writer.WriteStartElement(Serialization.Relation);
            this.writer.WriteAttributeString(Serialization.Association, Format(record.ObjectId));
            this.writer.WriteString(Format(record.GetCompositeRole(roleType)));
            this.writer.WriteEndElement();
        }
    }
}
