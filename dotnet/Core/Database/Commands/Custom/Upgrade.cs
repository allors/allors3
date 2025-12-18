// <copyright file="Upgrade.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Add file contents to the index")]
    public class Upgrade
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        [Option("-f", Description = "File to load")]
        public string FileName { get; set; } = "population.xml";

        public async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            var fileInfo = new FileInfo(this.FileName);

            this.Logger.Info("Begin");

            using (var client = this.Parent.ApiClient)
            {
                this.Logger.Info("Upgrading from {file}", fileInfo.FullName);
                using (var fileStream = File.OpenRead(fileInfo.FullName))
                {
                    var response = await client.UpgradeAsync(fileStream, fileInfo.Name);

                    if (!response.Success)
                    {
                        this.Logger.Error(response.ErrorMessage);

                        if (response.NotLoadedObjectTypeIds?.Length > 0)
                        {
                            var notLoaded = response.NotLoadedObjectTypeIds
                                .Aggregate("Could not load following ObjectTypeIds: ", (current, objectTypeId) => current + "- " + objectTypeId);
                            this.Logger.Error(notLoaded);
                        }

                        if (response.NotLoadedRelationTypeIds?.Length > 0)
                        {
                            var notLoaded = response.NotLoadedRelationTypeIds
                                .Aggregate("Could not load following RelationTypeIds: ", (current, relationTypeId) => current + "- " + relationTypeId);
                            this.Logger.Error(notLoaded);
                        }

                        return 1;
                    }
                }
            }

            this.Logger.Info("End");
            return ExitCode.Success;
        }
    }
}
