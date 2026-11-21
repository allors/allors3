// <copyright file="Upgrade.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using Allors.Database.Domain;
    using Allors.Database.Services;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Load a population that an earlier version saved, and bring it up to date with this version")]
    public class Upgrade
    {
        // The ids of the classes and of the relation types that this version removed on purpose. The load
        // leaves out their objects and relations, and the relations of those objects; anything else that it
        // cannot load stops the upgrade.
        private readonly HashSet<Guid> excludedObjectTypes = new HashSet<Guid>
        {
        };

        private readonly HashSet<Guid> excludedRelationTypes = new HashSet<Guid>
        {
        };

        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        [Option("-f", Description = "File to load (default is population.xml)")]
        public string FileName { get; set; }

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
                if (this.excludedObjectTypes.Contains(args.ObjectTypeId))
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
                if (!this.excludedRelationTypes.Contains(args.RelationTypeId) && !excludedObjects.Contains(args.AssociationId))
                {
                    notLoadedRelationTypeIds.Add(args.RelationTypeId);
                }
            };

            using (var reader = XmlReader.Create(fileInfo.FullName))
            {
                this.Logger.Info("Loading {file}", fileInfo.FullName);
                database.Load(reader);
            }

            if (notLoadedObjectTypeIds.Count > 0)
            {
                this.Logger.Error("The file holds objects of classes that this version does not have: {ids}. Add the id of a class that this version removed on purpose to the excluded object types of the upgrade command, and upgrade again.", string.Join(", ", notLoadedObjectTypeIds));
                return ExitCode.Error;
            }

            if (notLoadedRelationTypeIds.Count > 0)
            {
                this.Logger.Error("The file holds relations that this version could not load, of the relation types {ids}. Add the id of a relation type that this version removed on purpose to the excluded relation types of the upgrade command, and upgrade again.", string.Join(", ", notLoadedRelationTypeIds));
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
