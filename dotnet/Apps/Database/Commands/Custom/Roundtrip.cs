// <copyright file="Custom.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Allors.Database.Derivations;
    using Allors.Database.Meta;
    using Allors.Database.Services;
    using CaseExtensions;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;
    using Enumeration = Allors.Database.Domain.Enumeration;

    [Command(Description = "Roundtrip")]
    public class Roundtrip
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            using var transaction = this.Parent.Database.CreateTransaction();
            this.Logger.Info("Begin");

            var m = this.Parent.M;

            var excluded = new HashSet<IClass> { m.Country, m.Currency, m.Language };

            var populationElement = new XElement("population");

            var grouped = transaction.Extent<Enumeration>()
                .GroupBy(v => v.Strategy.Class);

            foreach (var group in grouped.OrderBy(v => v.Key.Name.ToPascalCase(), StringComparer.OrdinalIgnoreCase))
            {
                var @class = group.Key;

                if (excluded.Contains(@class))
                {
                    continue;
                }

                //if (Equals(@class, m.CostCenterSplitMethod))
                //{
                //    Debugger.Break();
                //}

                var classElement = new XElement(@class.PluralName.ToCamelCase());

                foreach (var @object in group.OrderBy(v => v.Name.ToPascalCase(), StringComparer.OrdinalIgnoreCase))
                {
                    var key = new string(@object.Name.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-' || c == '_').ToArray()).ToPascalCase();

                    var objectElement = new XElement(@class.SingularName.ToCamelCase(),
                        new XAttribute("handle", key),
                        new XElement("key", key));

                    classElement.Add(objectElement);
                }

                populationElement.Add(classElement);
            }

            var document = new XDocument(populationElement);

            var fileInfo = new FileInfo(@"\temp\records.xml");
            using var stream = File.Open(fileInfo.FullName, FileMode.Create);
            document.Save(stream);

            this.Logger.Info("End");

            return ExitCode.Success;
        }
    }
}
