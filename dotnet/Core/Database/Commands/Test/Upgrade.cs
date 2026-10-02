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
    using Microsoft.Extensions.Logging;

    [Command(Description = "Add file contents to the index")]
    public partial class Upgrade
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

        private readonly ILogger<Upgrade> logger;

        public Upgrade(ILogger<Upgrade> logger) => this.logger = logger;

        public Program Parent { get; set; }

        [Option("-f", Description = "File to load")]
        public string FileName { get; set; } = "population.xml";

        public int OnExecute(CommandLineApplication app)
        {
            var fileInfo = new FileInfo(this.FileName);

            this.LogBegin();

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
                    if (!this.excludedRelationTypes.Contains(args.RelationTypeId) && !notLoadedObjects.Contains(args.AssociationId))
                    {
                        notLoadedRelationTypeIds.Add(args.RelationTypeId);
                    }
                };

                this.LogLoading(fileInfo.FullName);
                this.Parent.Database.Load(reader);
            }

            if (notLoadedObjectTypeIds.Count > 0)
            {
                this.LogObjectTypesNotLoaded(string.Join(", ", notLoadedObjectTypeIds));
                return 1;
            }

            if (notLoadedRelationTypeIds.Count > 0)
            {
                this.LogRelationTypesNotLoaded(string.Join(", ", notLoadedRelationTypeIds));
                return 1;
            }

            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                this.Parent.Database.Services.Get<IPermissions>().Sync(transaction);

                new Allors.Database.Domain.Upgrade(transaction, this.Parent.DataPath).Execute();
                transaction.Commit();

                new Security(transaction).Apply();

                transaction.Commit();
            }

            this.LogEnd();
            return ExitCode.Success;
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Begin")]
        private partial void LogBegin();

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Loading {File}")]
        private partial void LogLoading(string file);

        [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Could not load the following object type ids: {ObjectTypeIds}")]
        private partial void LogObjectTypesNotLoaded(string objectTypeIds);

        [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Could not load the following relation type ids: {RelationTypeIds}")]
        private partial void LogRelationTypesNotLoaded(string relationTypeIds);

        [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "End")]
        private partial void LogEnd();
    }
}
