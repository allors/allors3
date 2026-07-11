// <copyright file="Custom.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Allors.Database;
    using Allors.Database.Meta;
    using Allors.Database.Population.Resx;
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

            var enumerations = transaction.Extent<Enumeration>().ToArray();

            string ToKey(string name) => new string(name.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-' || c == '_').ToArray()).ToPascalCase();

            {
                var populationElement = new XElement("population");

                var grouped = enumerations
                    .GroupBy(v => v.Strategy.Class);

                foreach (var group in grouped.OrderBy(v => ToKey(v.Key.Name), StringComparer.OrdinalIgnoreCase))
                {
                    var @class = group.Key;

                    if (excluded.Contains(@class))
                    {
                        continue;
                    }

                    var classElement = new XElement(@class.PluralName.ToCamelCase());

                    foreach (var @object in group.OrderBy(v => v.Name.ToPascalCase(), StringComparer.OrdinalIgnoreCase))
                    {
                        var name = @object.Name;
                        var key = ToKey(name);

                        var objectElement = new XElement(@class.SingularName.ToCamelCase(),
                            new XAttribute("handle", key),
                            new XElement("key", key));

                        classElement.Add(objectElement);
                    }

                    populationElement.Add(classElement);
                }

                var document = new XDocument(populationElement);

                var fileInfo = new FileInfo(@"\temp\enumerations.xml");

                this.Logger.Info(fileInfo.FullName);

                using var stream = File.Open(fileInfo.FullName, FileMode.Create);
                document.Save(stream);
            }

            {
                var populationElement = new XElement("population");

                var excludedEnumerations = new HashSet<IObject>(enumerations);
                var grouped = transaction.Extent<Allors.Database.Domain.ObjectState>()
                    .Where(v => !excludedEnumerations.Contains(v))
                    .GroupBy(v => v.Strategy.Class);

                foreach (var group in grouped.OrderBy(v => v.Key.Name.ToPascalCase(), StringComparer.OrdinalIgnoreCase))
                {
                    var @class = group.Key;

                    if (excluded.Contains(@class))
                    {
                        continue;
                    }

                    var classElement = new XElement(@class.PluralName.ToCamelCase());

                    foreach (var @object in group.OrderBy(v => v.Name.ToPascalCase(), StringComparer.OrdinalIgnoreCase))
                    {
                        var name = @object.Name;
                        var key = ToKey(name);

                        var objectElement = new XElement(@class.SingularName.ToCamelCase(),
                            new XAttribute("handle", key),
                            new XElement("key", key));

                        classElement.Add(objectElement);
                    }

                    populationElement.Add(classElement);
                }

                var document = new XDocument(populationElement);

                var fileInfo = new FileInfo(@"\temp\objectStates.xml");

                this.Logger.Info(fileInfo.FullName);

                using var stream = File.Open(fileInfo.FullName, FileMode.Create);
                document.Save(stream);
            }

            {
                var directoryInfo = new DirectoryInfo(@"\temp\translations");
                if (directoryInfo.Exists)
                {
                    directoryInfo.Delete(true);
                }

                directoryInfo.Create();

                this.Logger.Info(directoryInfo.FullName);

                {
                    var translationsByIsoCodeByClass = enumerations
                        .GroupBy(v => v.Strategy.Class)
                        .ToDictionary(v => v.Key, v => v
                                .SelectMany(w => w.LocalisedNames)
                                .Select(w => new Translation(v.Key, w.Locale.Language.IsoCode, ToKey(w.EnumerationWhereLocalisedName.Name), w.Text))
                                .GroupBy(w => w.IsoCode)
                                .ToDictionary(w => w.Key, w => new Translations(v.Key, w.Key, w.ToDictionary(x => x.Key, x => x.Value))) as IDictionary<string, Translations>
                        );

                    var translationsToFile = new TranslationsToFile(directoryInfo, translationsByIsoCodeByClass, m.Enumeration.LocalisedNames);

                    translationsToFile.Roundtrip();
                }

                {
                    var translationsByIsoCodeByClass = enumerations
                        .GroupBy(v => v.Strategy.Class)
                        .ToDictionary(v => v.Key, v =>
                           new Dictionary<string, Translations>
                           {
                               {
                                   "en", new Translations(v.Key, "en", v.OrderBy(w=>w.Name).ToDictionary(w=>ToKey(w.Name), w=>w.Name))
                               }
                           } as IDictionary<string, Translations>
                        );

                    var translationsToFile = new TranslationsToFile(directoryInfo, translationsByIsoCodeByClass, m.Enumeration.LocalisedNames);

                    translationsToFile.Roundtrip();
                }

            }

            this.Logger.Info("End");

            return ExitCode.Success;
        }
    }

}
