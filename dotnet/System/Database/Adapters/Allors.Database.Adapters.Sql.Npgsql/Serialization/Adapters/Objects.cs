// <copyright file="Objects.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml;
    using Adapters;
    using Meta;

    public class Objects : IEnumerable<object[]>
    {
        private readonly Database database;
        private readonly Action<Guid, long> onObjectNotLoaded;
        private readonly Dictionary<long, IClass> classByObjectId;
        private readonly XmlReader reader;

        public Objects(
            Database database,
            Action<Guid, long> onObjectNotLoaded,
            Dictionary<long, IClass> classByObjectId,
            XmlReader reader)
        {
            this.database = database;
            this.onObjectNotLoaded = onObjectNotLoaded;
            this.classByObjectId = classByObjectId;
            this.reader = reader;
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            // The reader holds the database element and what is in it: move to the element, so that the loop
            // reads its children.
            this.reader.MoveToContent();

            var skip = false;
            while (skip || this.reader.Read())
            {
                skip = false;

                switch (this.reader.NodeType)
                {
                    // eat everything but elements
                    case XmlNodeType.Element:
                        if (this.reader.Name.Equals(Serialization.ObjectType))
                        {
                            // A self-closing element holds no objects, and the next Read moves past it.
                            if (!this.reader.IsEmptyElement)
                            {
                                var objectTypeIdString = this.reader.GetAttribute(Serialization.Id);
                                if (string.IsNullOrEmpty(objectTypeIdString))
                                {
                                    throw new Exception("object type has no id");
                                }

                                var objectTypeId = new Guid(objectTypeIdString);
                                var objectType = this.database.ObjectFactory.GetObjectType(objectTypeId);

                                var objectIdsString = this.reader.ReadElementContentAsString();

                                // An element with an end tag and no content holds no objects, as a self-closing element does.
                                var objectIdStrings = objectIdsString.Length > 0
                                    ? objectIdsString.Split(Serialization.ObjectsSplitterCharArray)
                                    : Array.Empty<string>();

                                foreach (var objectIdString in objectIdStrings)
                                {
                                    var objectArray = objectIdString.Split(Serialization.ObjectSplitterCharArray);

                                    var objectId = long.Parse(objectArray[0]);
                                    var objectVersion = objectArray.Length > 1
                                        ? long.Parse(objectArray[1])
                                        : (long) Allors.Version.DatabaseInitial;

                                    if (objectType is IClass @class)
                                    {
                                        this.classByObjectId[objectId] = @class;
                                        yield return new object[] { objectId, @class.Id, objectVersion };
                                    }
                                    else
                                    {
                                        this.onObjectNotLoaded(objectTypeId, objectId);
                                    }
                                }

                                skip = this.reader.IsStartElement();
                            }
                        }
                        else
                        {
                            throw new Exception("Unknown child element <" + this.reader.Name + "> in parent element <" + Serialization.Database + ">");
                        }

                        break;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
