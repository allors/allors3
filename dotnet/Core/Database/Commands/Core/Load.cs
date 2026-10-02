// <copyright file="Import.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System.IO;
    using System.Xml;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Extensions.Logging;

    [Command(Description = "Import the population from file")]
    public partial class Load
    {
        private readonly ILogger<Load> logger;

        public Load(ILogger<Load> logger) => this.logger = logger;

        public Program Parent { get; set; }

        [Option("-f", Description = "File to load (default is population.xml)")]
        public string FileName { get; set; }

        public int OnExecute(CommandLineApplication app)
        {
            this.LogBegin();

            var fileName = this.FileName ?? this.Parent.Configuration["populationFile"] ?? "population.xml";
            var fileInfo = new FileInfo(fileName);

            using (var reader = XmlReader.Create(fileInfo.FullName))
            {
                this.LogLoading(fileInfo.FullName);
                this.Parent.Database.Load(reader);
            }

            this.LogEnd();
            return ExitCode.Success;
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Begin")]
        private partial void LogBegin();

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Loading {File}")]
        private partial void LogLoading(string file);

        [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "End")]
        private partial void LogEnd();
    }
}
