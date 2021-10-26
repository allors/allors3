namespace Allors.Meta.Generation.Model
{
    using System;

    public interface IMetaIdentifiableObjectModel
    {
        Guid Id { get; }

        string Tag { get; }
    }
}
