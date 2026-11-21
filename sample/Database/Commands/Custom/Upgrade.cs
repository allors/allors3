// <copyright file="Upgrade.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using Allors.Database.Domain;
    using Allors.Database.Services;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;
    using IRelationType = Allors.Database.Meta.IRelationType;

    [Command(Description = "Load a population that an earlier version saved, and bring it up to date with this version")]
    public class Upgrade
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        [Option("-f", Description = "File to load (default is population.xml)")]
        public string FileName { get; set; }

        // The ids of the classes and of the relation types that this version removed on purpose. The load leaves
        // out their objects and relations, the relations of those objects, and those objects in the roles of the
        // objects that it loads; anything else that it cannot load stops the upgrade.
        public HashSet<Guid> ExcludedObjectTypes { get; } = new HashSet<Guid>
        {
        };

        public HashSet<Guid> ExcludedRelationTypes { get; } = new HashSet<Guid>
        {
        };

        public int OnExecute(CommandLineApplication app)
        {
            this.Logger.Info("Begin");

            var fileName = this.FileName ?? this.Parent.Configuration["populationFile"] ?? "population.xml";
            var fileInfo = new FileInfo(fileName);

            var notLoadedObjectTypeIds = new HashSet<Guid>();
            var notLoadedRelationTypeIds = new HashSet<Guid>();
            var excludedObjects = new HashSet<long>();

            var database = this.Parent.Database;

            database.ObjectNotLoaded += (sender, args) =>
            {
                if (this.ExcludedObjectTypes.Contains(args.ObjectTypeId))
                {
                    excludedObjects.Add(args.ObjectId);
                }
                else
                {
                    notLoadedObjectTypeIds.Add(args.ObjectTypeId);
                }
            };

            database.RelationNotLoaded += (sender, args) =>
            {
                if (this.ExcludedRelationTypes.Contains(args.RelationTypeId) || excludedObjects.Contains(args.AssociationId))
                {
                    return;
                }

                // The load reports each object that it leaves out of a composite role on its own, and keeps the
                // other objects of the role. The contents of a unit role can look like the id of an object, so only
                // a composite role is compared with the objects left out.
                if (database.MetaPopulation.FindById(args.RelationTypeId) is IRelationType { RoleType.ObjectType.IsComposite: true }
                    && long.TryParse(args.RoleContents, NumberStyles.None, CultureInfo.InvariantCulture, out var roleId)
                    && excludedObjects.Contains(roleId))
                {
                    return;
                }

                notLoadedRelationTypeIds.Add(args.RelationTypeId);
            };

            using (var reader = XmlReader.Create(fileInfo.FullName))
            {
                this.Logger.Info("Loading {file}", fileInfo.FullName);
                database.Load(reader);
            }

            if (notLoadedObjectTypeIds.Count > 0 || notLoadedRelationTypeIds.Count > 0)
            {
                if (notLoadedObjectTypeIds.Count > 0)
                {
                    this.Logger.Error("The file holds objects of classes that this version does not have: {ids}. Add the id of a class that this version removed on purpose to the excluded object types of the upgrade command, and upgrade again.", string.Join(", ", notLoadedObjectTypeIds));
                }

                if (notLoadedRelationTypeIds.Count > 0)
                {
                    this.Logger.Error("The file holds relations that this version could not load, of the relation types {ids}. Add the id of a relation type that this version removed on purpose to the excluded relation types of the upgrade command, and upgrade again.", string.Join(", ", notLoadedRelationTypeIds));
                }

                // The load has replaced the population with the part of the file that it could load. The database
                // is left as a load that fails leaves it, as Init leaves it, so that no part of the file passes for
                // an upgraded population.
                database.Init();
                this.Logger.Error("The upgrade stopped and left the database empty.");
                return ExitCode.Error;
            }

            using (var transaction = database.CreateTransaction())
            {
                // The permissions follow the model of this version: new ones for new classes, relation types
                // and method types, none for what this version removed.
                database.Services.Get<IPermissions>().Sync(transaction);

                new Allors.Database.Domain.Upgrade(transaction).Execute();
                transaction.Derive();
                transaction.Commit();

                // The security roles get their permissions again, the new ones included.
                new Security(transaction).Apply();
                transaction.Commit();
            }

            this.Logger.Info("End");
            return ExitCode.Success;
        }
    }
}
