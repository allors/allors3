// <copyright file="Save.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System.IO;
    using System.Xml;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Extensions.Logging;

    [Command(Description = "Save the population to file")]
    public partial class Save
    {
        private readonly ILogger<Save> logger;

        public Save(ILogger<Save> logger) => this.logger = logger;

        public Program Parent { get; set; }

        [Option("-f", Description = "File to save")]
        public string FileName { get; set; } = "population.xml";

        public int OnExecute(CommandLineApplication app)
        {
            this.LogBegin();

            var fileName = this.FileName ?? this.Parent.Configuration["populationFile"];
            var fileInfo = new FileInfo(fileName);

            using (var stream = File.Create(fileInfo.FullName))
            {
                using (var writer = XmlWriter.Create(stream))
                {
                    this.LogSaving(fileInfo.FullName);
                    this.Parent.Database.Save(writer);
                }
            }

            this.LogEnd();
            return ExitCode.Success;
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Begin")]
        private partial void LogBegin();

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Saving {File}")]
        private partial void LogSaving(string file);

        [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "End")]
        private partial void LogEnd();
    }
}
