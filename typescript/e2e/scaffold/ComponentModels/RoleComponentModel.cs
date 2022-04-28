namespace Scaffold
{
    using AngleSharp.Dom;

    public class RoleComponentModel : ComponentModel
    {
        public static readonly Dictionary<string, string> TypeByTag = new()
        {
            { "a-mat-input", "Allors.E2E.Angular.Material.Role.AllorsMaterialInputComponent" }
        };

        public override string Property { get; }

        public override string Type { get; }

        public override string Init { get; }

        public RoleComponentModel(IElement element)
        {
            var tag = element.TagName.ToLowerInvariant();
            var fullType = TypeByTag[tag];

            var roleTypeAttribute = element.GetAttribute("[roleType]");
            var roleTypeParts = roleTypeAttribute.Split(".");
            var roleTypeObjectType = roleTypeParts[1];
            var roleTypeName = roleTypeParts[2];

            this.Type = fullType;
            this.Property = $"{roleTypeName}";
            this.Init = $"new {fullType}(this, this.M.{roleTypeObjectType}.{roleTypeName});";
        }
    }
}
