namespace Allors.Database.Population.Resx
{
    using System.Collections.Generic;
    using Meta;

    public record Translation(
        IClass Class,
        string IsoCode,
        string Key,
        string Value);
}
