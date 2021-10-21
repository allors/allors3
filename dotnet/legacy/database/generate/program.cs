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

    class Program
    {
        private static readonly MetaBuilder MetaBuilder = new MetaBuilder();

        static int Main()
        {
            string[,] database =
                {
                    { "../Core/Database/Templates/domain.cs.stg", "DataBase/Domain/generated" },
                    { "../Core/Database/Templates/uml.cs.stg", "DataBase/Diagrams/generated" },
                };

            string[,] workspace =
            {
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

            var workspaceName = "Default";

            for (var i = 0; i < workspace.GetLength(0); i++)
            {
                var template = workspace[i, 0];
                var output = workspace[i, 1];

                Console.WriteLine("-> " + output);

                RemoveDirectory(output);

                var log = Generate.Execute(model, template, output, workspaceName);
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
