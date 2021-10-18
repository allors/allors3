// <copyright file="Program.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All Rights Reserved.
// Licensed under the LGPL v3 license.
// </copyright>

using System.Text.Json;
using System.Threading.Tasks;
using Tests;

namespace Allors
{
    using System;
    using System.IO;
    using Allors.Development.Repository.Tasks;
    using Autotest;
    using Workspace.Meta.Lazy;

    internal class Program
    {
        private static int Default(Model model)
        {
            try
            {
                string[,] config =
                {
                    {
                        "../Base/Templates/sidenav.cs.stg", "./Angular.Tests/generated/sidenav",
                    },
                    {
                        "../Base/Templates/component.cs.stg", "./Angular.Tests/generated/components",
                    },
                };

                for (var i = 0; i < config.GetLength(0); i++)
                {
                    var template = config[i, 0];
                    var output = config[i, 1];

                    Console.WriteLine($"{template} -> {output}");

                    RemoveDirectory(output);

                    var log = Generate.Execute(template, output, model);
                    if (log.ErrorOccured)
                    {
                        return 1;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e + e.StackTrace);
            }

            return 0;
        }

        private static async Task<int> Main(string[] args)
        {
            int result;

            try
            {
                var model = new Model
                {
                    MetaPopulation = new MetaBuilder().Build(),
                };

                const string location = "../../modules/dist/base";
                model.LoadProject(new FileInfo($"{location}/project.json"));

                using var driverManager = new DriverManager();
                driverManager.Start();
                var driver = driverManager.Driver;

                driver.Navigate().GoToUrl("http://localhost:4200/_allors");
                var json = driver.GetGlobal("allors");
                var info = JsonSerializer.Deserialize<AllorsInfo>(json);
                model.LoadMetaExtensions(info);
                model.LoadMenu(info);
                model.LoadDialogs(info);

                result = args.Length switch
                {
                    0 => Default(model),
                    2 => Generate.Execute(args[0], args[1], model).ErrorOccured ? 1 : 0,
                    _ => 1
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                result = 1;
            }

            return result;
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
