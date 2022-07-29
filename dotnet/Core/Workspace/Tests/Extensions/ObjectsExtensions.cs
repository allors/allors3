namespace Tests.Workspace
{
    using System.Collections.Generic;
    using Allors.Workspace;

    public static class ObjectsExtensions
    {
        public static string Dump(this IEnumerable<IObject> collection)
            => collection != null ? "[" + string.Join(",", collection) + "]" : "null";
    }
}
