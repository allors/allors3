// <copyright file="MigrateMediaToFile.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using Allors.Database.Domain;
    using Allors.Database.Services;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Replaces every Media's InlineMediaContent with a FileMediaContent and switches new media to file storage.")]
    public class MigrateMediaToFile
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            using var transaction = this.Parent.Database.CreateTransaction();

            var scheduler = new AutomatedAgents(transaction).System;
            transaction.Services.Get<IUserService>().User = scheduler;

            // New media should be file-backed from now on.
            transaction.GetSingleton().StoreMediaContentOnFile = true;

            var converted = Medias.ConvertInlineMediaContentToFile(transaction);

            transaction.Derive();
            transaction.Commit();

            this.Logger.Info($"Converted {converted} media to file-backed content; new media will use file storage.");

            return ExitCode.Success;
        }
    }
}
