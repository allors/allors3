namespace Allors.Database.Population.Resx
{
    using System.Collections.Generic;
    using Meta;

    public record Translations(
        IClass Class,
        string IsoCode,
        IDictionary<string, string> ValueByKey);
}
