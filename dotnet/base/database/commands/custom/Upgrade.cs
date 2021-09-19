// <copyright file="Upgrade.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Allors.Database.Domain;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Add file contents to the index")]
    public class Upgrade
    {
        private readonly HashSet<Guid> excludedObjectTypes = new HashSet<Guid>
        {
        };

        private readonly HashSet<Guid> excludedRelationTypes = new HashSet<Guid>
        {
        };

        private readonly HashSet<Guid> movedRelationTypes = new HashSet<Guid>
        {
        };

        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        [Option("-f", Description = "File to load")]
        public string FileName { get; set; } = "population.xml";

        public int OnExecute(CommandLineApplication app)
        {
            var fileInfo = new FileInfo(this.FileName);

            this.Logger.Info("Begin");

            var notLoadedObjectTypeIds = new HashSet<Guid>();
            var notLoadedRelationTypeIds = new HashSet<Guid>();

            var notLoadedObjects = new HashSet<long>();

            using (var reader = XmlReader.Create(fileInfo.FullName))
            {
                this.Parent.Database.ObjectNotLoaded += (sender, args) =>
                {
                    if (!this.excludedObjectTypes.Contains(args.ObjectTypeId))
                    {
                        notLoadedObjectTypeIds.Add(args.ObjectTypeId);
                    }
                    else
                    {
                        var id = args.ObjectId;
                        notLoadedObjects.Add(id);
                    }
                };

                this.Parent.Database.RelationNotLoaded += (sender, args) =>
                {
                    if (!this.excludedRelationTypes.Contains(args.RelationTypeId))
                    {
                        if (!notLoadedObjects.Contains(args.AssociationId))
                        {
                            notLoadedRelationTypeIds.Add(args.RelationTypeId);
                        }
                    }
                };

                this.Logger.Info("Loading {file}", fileInfo.FullName);
                this.Parent.Database.Load(reader);
            }

            if (notLoadedObjectTypeIds.Count > 0)
            {
                var notLoaded = notLoadedObjectTypeIds
                    .Aggregate("Could not load following ObjectTypeIds: ", (current, objectTypeId) => current + "- " + objectTypeId);

                this.Logger.Error(notLoaded);
                return 1;
            }

            if (notLoadedRelationTypeIds.Count > 0)
            {
                var notLoaded = notLoadedRelationTypeIds
                    .Aggregate("Could not load following RelationTypeIds: ", (current, relationTypeId) => current + "- " + relationTypeId);

                this.Logger.Error(notLoaded);
                return 1;
            }

            Permissions.Sync(this.Parent.Database);

            using (var session = this.Parent.Database.CreateTransaction())
            {
                new Allors.Database.Domain.Upgrade(session, this.Parent.DataPath).Execute();
                session.Commit();

                new Security(session).Apply();

                session.Commit();
            }

            this.Logger.Info("End");
            return ExitCode.Success;
        }
    }
}
