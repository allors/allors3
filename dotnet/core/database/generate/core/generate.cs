// -------------------------------------------------------------------------------------------------
// <copyright file="Generate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// -------------------------------------------------------------------------------------------------
namespace Allors.Development.Repository.Tasks
{
    using System.IO;
    using Database.Meta;
    using StringTemplate = Generation.StringTemplate;

    public static class Generate
    {
        public static Log Execute(MetaPopulation metaPopulation, string template, string output, string workspaceName = null)
        {
            var log = new GenerateLog();

            var templateFileInfo = new FileInfo(template);
            var stringTemplate = new StringTemplate(templateFileInfo);
            var outputDirectoryInfo = new DirectoryInfo(output);

            stringTemplate.Generate(metaPopulation, workspaceName, outputDirectoryInfo, log);

            return log;
        }
    }
}
