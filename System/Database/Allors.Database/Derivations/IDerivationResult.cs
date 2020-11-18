namespace Allors.Database.Derivations
{
    public interface IDerivationResult
    {
        bool HasErrors { get; }

        IDerivationError[] Errors { get; }
    }
}
