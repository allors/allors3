namespace Application
{
    using Allors.Workspace;
    using Allors.Workspace.Meta;

    public interface ITransformation
    {
        object ToExcel(IObject @object, IRoleType roleType);

        object ToDomain(dynamic value);
    }
}
