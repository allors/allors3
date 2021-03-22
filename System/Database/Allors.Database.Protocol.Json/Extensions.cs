// <copyright file="FromJsonVisitor.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using Allors.Protocol.Json.Data;
    using Data;
    using Meta;
    using Extent = Data.Extent;
    using Pull = Allors.Protocol.Json.Data.Pull;
    using Select = Data.Select;

    public static class Extensions
    {
        public static IAssociationType FindAssociationType(this IMetaPopulation @this, Guid? id) => id != null ? ((IRelationType)@this.Find(id.Value)).AssociationType : null;

        public static IRoleType FindRoleType(this IMetaPopulation @this, Guid? id) => id != null ? ((IRelationType)@this.Find(id.Value)).RoleType : null;

        public static Data.Pull FromJson(this Pull pull, ITransaction transaction)
        {
            var fromJsonVisitor = new FromJsonVisitor(transaction);
            pull.Accept(fromJsonVisitor);
            return fromJsonVisitor.Pull;
        }

        public static IDictionary<string, IObject[]> FromJsonForCollectionByName(this string[][] collectionByName, ITransaction transaction) =>
            collectionByName?.Select(namedCollection =>
            {
                var key = namedCollection[0];

                if (namedCollection.Length <= 1)
                {
                    return new KeyValuePair<string, IObject[]>(key, null);
                }

                var ids = namedCollection.Skip(1);
                var value = transaction.Instantiate(ids);

                return new KeyValuePair<string, IObject[]>(key, value);

            }).ToDictionary(v => v.Key, v => v.Value);

        public static IDictionary<string, IObject> FromJsonForObjectByName(this string[][] collectionByName, ITransaction transaction) =>
            collectionByName?.Select(namedCollection =>
            {
                var key = namedCollection[0];

                if (namedCollection.Length <= 1)
                {
                    return new KeyValuePair<string, IObject>(key, null);
                }

                var value = transaction.Instantiate(namedCollection[1]);

                return new KeyValuePair<string, IObject>(key, value);
            }).ToDictionary(v => v.Key, v => v.Value);

        public static IDictionary<IObject, long> FromJsonForVersionByObject(this string[][] versionByObject, ITransaction transaction) =>
            versionByObject?.Select(versionByObject =>
            {
                var key = transaction.Instantiate(versionByObject[0]);
                var value = long.Parse(versionByObject[1]);
                return new KeyValuePair<IObject, long>(key, value);

            }).ToDictionary(v => v.Key, v => v.Value);

        public static Data.Procedure FromJson(this Allors.Protocol.Json.Data.Procedure procedure, ITransaction transaction)
        {
            var fromJsonVisitor = new FromJsonVisitor(transaction);
            procedure.Accept(fromJsonVisitor);
            return fromJsonVisitor.Procedure;
        }

        public static Pull ToJson(this Data.Pull pull)
        {
            var toJsonVisitor = new ToJsonVisitor();
            pull.Accept(toJsonVisitor);
            return toJsonVisitor.Pull;
        }

        public static Allors.Protocol.Json.Data.Procedure ToJson(this Data.Procedure procedure)
        {
            var toJsonVisitor = new ToJsonVisitor();
            procedure.Accept(toJsonVisitor);
            return toJsonVisitor.Procedure;
        }

        public static string[][] ToJsonForCollectionByName(this IDictionary<string, IObject[]> collectionByName) =>
            collectionByName?.Select(kvp =>
            {
                var name = kvp.Key;
                var collection = kvp.Value;

                if (collection == null || collection.Length == 0)
                {
                    return new[] { name };
                }

                var jsonCollection = new string[collection.Length + 1];
                jsonCollection[0] = name;

                for (var i = 0; i < collection.Length; i++)
                {
                    jsonCollection[i + 1] = collection[i].Id.ToString();
                }

                return jsonCollection;
            }).ToArray();

        public static string[][] ToJsonForObjectByName(this IDictionary<string, IObject> objectByName) =>
            objectByName?.Select(kvp =>
            {
                var name = kvp.Key;
                var @object = kvp.Value;

                return @object == null ? new[] { name } : new[] { name, @object.Id.ToString() };
            }).ToArray();

        public static string[][] ToJsonForVersionByObject(this IDictionary<IObject, long> versionByObject) =>
            versionByObject?.Select(kvp => new[] { kvp.Key.Id.ToString(), kvp.Value.ToString() }).ToArray();
    }
}
