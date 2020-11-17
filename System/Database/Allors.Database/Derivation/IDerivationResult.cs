namespace Allors
{
    public interface IDerivationResult
    {
        bool HasErrors { get; }

        IDerivationError[] Errors { get; }
    }
}
