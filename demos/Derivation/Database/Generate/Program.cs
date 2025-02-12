// <copyright file="Program.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Allors.Meta.Generation.Model;

namespace Allors.Meta.Generation
{
    using System;
    using System.IO;
    using Database.Meta;

    internal class Program
    {
        private static readonly MetaBuilder MetaBuilder = new MetaBuilder();

        private static int Main()
        {
            string[,] database =
            {
                { "../../dotnet/Core/Database/Templates/domain.cs.stg", "Database/Domain/Generated" },
                { "../../dotnet/Core/Database/Templates/uml.cs.stg", "Database/Diagrams/Generated" },
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
