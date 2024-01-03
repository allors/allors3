namespace Allors.Database.Population.Resx
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Database.Meta;
    using Database.Population;
    using KGySoft.Resources;

    public partial class TranslationsToFile
    {
        private readonly DirectoryInfo directoryInfo;
        private readonly IRoleType roleType;

        public TranslationsToFile(
            DirectoryInfo directoryInfo,
            IDictionary<IClass, IDictionary<string, Translations>> translationsByIsoCodeByClass,
            IRoleType roleType)
        {
            this.directoryInfo = directoryInfo;
            this.roleType = roleType;
            this.TranslationsByIsoCodeByClass = translationsByIsoCodeByClass;
        }

        public IDictionary<IClass, IDictionary<string, Translations>> TranslationsByIsoCodeByClass { get; }

        public void Roundtrip()
        {
            foreach (var (@class, translationsByIsoCode) in this.TranslationsByIsoCodeByClass)
            {
                foreach (var (isoCode, translations) in translationsByIsoCode)
                {
                    var fileName = isoCode.Equals("en", System.StringComparison.OrdinalIgnoreCase)
                        ? $"{@class.SingularName}.{this.roleType.SingularName}.resx"
                        : $"{@class.SingularName}.{this.roleType.SingularName}.{isoCode}.resx";

                    var fileInfo = new FileInfo(Path.Combine(this.directoryInfo.FullName, fileName));

                    using var writer = new ResXResourceWriter(fileInfo.FullName);

                    var valueByKey = translations.ValueByKey;
                    foreach (var (key, value) in valueByKey.OrderBy(v => v.Key))
                    {
                        writer.AddResource(key, value);
                    }
                }
            }
        }
    }
}
