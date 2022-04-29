namespace Scaffold
{
    using AngleSharp.Dom;

    public class DynamicExtentComponentModel : ComponentModel
    {
        public static readonly Dictionary<string, string> TypeByTag = new()
        {
            { "a-mat-dyn-edit-extent-panel", "Allors.E2E.Angular.Material.DynamicEditExtentPanelComponent" },
            { "a-mat-dyn-view-extent-panel", "Allors.E2E.Angular.Material.DynamicViewExtentPanelComponent" },
        };

        public override string Property { get; }

        public override string Type { get; }

        public override string Init { get; }

        public DynamicExtentComponentModel(IElement element)
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

        public class Builder : ComponentModelBuilder
        {
            public Builder(ComponentModelBuilder? next = null) : base(next)
            {
            }

            public override ComponentModel? Build(IElement element) =>
                TypeByTag.ContainsKey(element.TagName.ToLowerInvariant())
                    ? new RoleComponentModel(element)
                    : base.Build(element);
        }
    }
}
