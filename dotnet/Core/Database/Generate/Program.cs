// <copyright file="Program.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Meta.Generation
{
    using System;
    using System.IO;
    using Database.Meta;
    using Model;

    internal class Program
    {
        private static readonly MetaBuilder MetaBuilder = new MetaBuilder();

        private static int Main()
        {
            string[,] database =
                {
                    { "Database/Templates/domain.cs.stg", "Database/Domain/Generated" },
                    { "Database/Templates/uml.cs.stg", "Database/Domain.Diagrams/Generated" },
                };

            string[,] workspace =
            {
                //{ "Workspace/Templates/uml.cs.stg", "Workspace/Diagrams/Generated" },
                { "Workspace/Templates/meta.cs.stg", "Workspace/Meta/Generated" },
                { "Workspace/Templates/meta.lazy.cs.stg", "Workspace/Meta.Lazy/Generated" },
                { "Workspace/Templates/domain.cs.stg", "Workspace/Domain/Generated" },

                { "../../typescript/modules/templates/workspace.meta.ts.stg", "../../typescript/modules/libs/core/workspace/meta/src/lib/generated" },
                { "../../typescript/modules/templates/workspace.meta.json.ts.stg", "../../typescript/modules/libs/core/workspace/meta-json/src/lib/generated" },
                { "../../typescript/modules/templates/workspace.domain.ts.stg", "../../typescript/modules/libs/core/workspace/domain/src/lib/generated" },
            };

            var metaPopulation = MetaBuilder.Build();
            var model = new MetaModel(metaPopulation);

            for (var i = 0; i < database.GetLength(0); i++)
            {
                var template = database[i, 0];
                var output = database[i, 1];

                Console.WriteLine("-> " + output);

                RemoveDirectory(output);

                var log = Generate.Execute(model, template, output);
                if (log.ErrorOccured)
                {
                    return 1;
                }
            }

            for (var i = 0; i < workspace.GetLength(0); i++)
            {
                var template = workspace[i, 0];
                var output = workspace[i, 1];

                Console.WriteLine("-> " + output);

                RemoveDirectory(output);

                var log = Generate.Execute(model, template, output, "Default");
                if (log.ErrorOccured)
                {
                    return 1;
                }
            }

            return 0;
        }

        private static void RemoveDirectory(string output)
        {
            var directoryInfo = new DirectoryInfo(output);
            if (directoryInfo.Exists)
            {
                try
                {
                    directoryInfo.Delete(true);
                }
                catch
                {
                }
            }
        }
    }
}
