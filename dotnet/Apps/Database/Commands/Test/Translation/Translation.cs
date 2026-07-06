namespace Allors.Database.Population.Resx
{
    using Meta;

    public record Translation(
        IClass Class,
        string IsoCode,
        string Key,
        string Value);
}
