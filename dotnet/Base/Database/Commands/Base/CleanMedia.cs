// <copyright file="CleanMedia.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using Allors.Database.Domain;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Removes orphaned file-backed media (files below the highest content id with no live content).")]
    public class CleanMedia
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            using var transaction = this.Parent.Database.CreateTransaction();

            var removed = FileMediaContents.RemoveOrphanedFiles(transaction);
            this.Logger.Info($"Removed {removed} orphaned media file(s).");

            return ExitCode.Success;
        }
    }
}
