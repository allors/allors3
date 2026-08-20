// <copyright file="Import.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System.IO;
    using System.Xml;
    using Allors.Database;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Import the population from file")]
    public class Load
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        [Option("-f", Description = "File to load (default is population.xml)")]
        public string FileName { get; set; }

        [Option("--v1-strings", Description = "String encoding (Raw|Base64) of the string unit roles, required for a version 1 population")]
        public StringEncoding? Version1Strings { get; set; }

        public int OnExecute(CommandLineApplication app)
        {
            this.Logger.Info("Begin");

            var fileName = this.FileName ?? this.Parent.Configuration["populationFile"] ?? "population.xml";
            var fileInfo = new FileInfo(fileName);

            using (var reader = XmlReader.Create(fileInfo.FullName))
            {
                this.Logger.Info("Loading {file}", fileInfo.FullName);
                this.Parent.Database.Load(reader, new LoadOptions { Version1StringEncoding = this.Version1Strings });
            }

            this.Logger.Info("End");
            return ExitCode.Success;
        }
    }
}
