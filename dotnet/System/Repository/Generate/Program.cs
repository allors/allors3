// <copyright file="Program.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Tools.Cmd
{
    using System;
    using System.IO;
    using Microsoft.Extensions.Logging;
    using Repository;
    using Repository.Roslyn;

    public partial class Program
    {
        public static int Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            }));
            var logger = loggerFactory.CreateLogger<Program>();

            try
            {
                if (args.Length < 3)
                {
                    LogMissingArguments(logger);
                }

                RepositoryGenerate(args, loggerFactory, logger);
            }
            catch (RepositoryException e)
            {
                LogRepositoryError(logger, e.Message);
                return 1;
            }
            catch (Exception e)
            {
                LogFinishedWithErrors(logger, e);
                return 1;
            }

            LogFinished(logger);
            return 0;
        }

        private static void RepositoryGenerate(string[] args, ILoggerFactory loggerFactory, ILogger logger)
        {
            var projectPath = args[0];
            var template = args[1];
            var output = args[2];

            var fileInfo = new FileInfo(projectPath);

            LogGenerating(logger, fileInfo.FullName);
            Generate.Execute(fileInfo.FullName, template, output, loggerFactory);
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "missing required arguments")]
        private static partial void LogMissingArguments(ILogger logger);

        [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "{ErrorMessage}")]
        private static partial void LogRepositoryError(ILogger logger, string errorMessage);

        [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Finished with errors")]
        private static partial void LogFinishedWithErrors(ILogger logger, Exception exception);

        [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Finished")]
        private static partial void LogFinished(ILogger logger);

        [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Generate {Project}")]
        private static partial void LogGenerating(ILogger logger, string project);
    }
}
