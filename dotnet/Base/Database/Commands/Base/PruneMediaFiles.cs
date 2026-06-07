// <copyright file="PruneMediaFiles.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using Allors.Database.Domain;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Removes orphaned media files (files below the highest live content id that no live content owns).")]
    public class PruneMediaFiles
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            using var transaction = this.Parent.Database.CreateTransaction();

            var removed = ExternalMediaContents.RemoveOrphanedFiles(transaction);
            this.Logger.Info($"Removed {removed} orphaned media file(s).");

            return ExitCode.Success;
        }
    }
}
