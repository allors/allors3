namespace Scaffold
{
    using AngleSharp.Dom;

    public class ExtentComponentModel : ComponentModel
    {
        public static string DataTag => "data-tag";

        public static readonly Dictionary<string, string> TypeByTag = new()
        {
            { "a-mat-dyn-view-extent-panel", "Allors.E2E.Angular.Material.Dynamic.AllorsMaterialDynamicViewExtentPanelComponent" },
            { "a-mat-dyn-edit-extent-panel", "Allors.E2E.Angular.Material.Dynamic.AllorsMaterialDynamicEditExtentPanelComponent" },
        };

        public static readonly Dictionary<string, string> PrefixByTag = new()
        {
            { "a-mat-dyn-view-extent-panel", "View" },
            { "a-mat-dyn-edit-extent-panel", "Edit" },
        };

        public override string Property { get; protected set; }

        public override string Type { get; }

        public override string Init { get; protected set; }

        private string FullProperty { get; }

        public override void Elevate(ISet<string> properties)
        {
            if (properties.Contains(this.Property))
            {
                this.Property = this.FullProperty;
            }
        }

        public ExtentComponentModel(IElement element)
        {
            var tag = element.TagName.ToLowerInvariant();
            var fullType = TypeByTag[tag];

            var prefix = PrefixByTag[tag];
            var dataTag = element.GetAttribute(DataTag)?.Trim();
            this.Property = $"{prefix}{dataTag}";
            this.FullProperty = this.Property;

            this.Type = fullType;
            this.Init = $"new {fullType}(this, \"{dataTag}\");";
        }

        public class Builder : ComponentModelBuilder
        {
            public Builder(ComponentModelBuilder? next = null) : base(next)
            {
            }

            public override ComponentModel? Build(IElement element) =>
                TypeByTag.ContainsKey(element.TagName.ToLowerInvariant()) && element.HasAttribute(DataTag)
                    ? new ExtentComponentModel(element)
                    : base.Build(element);
        }

        public override bool Equals(object? obj)
        {
            if (obj is RoleComponentModel that)
            {
                return string.Equals(this.Property, that.Property) &&
                       string.Equals(this.Type, that.Type) &&
                       string.Equals(this.Init, that.Init);
            }

            return base.Equals(obj);
        }
    }
}
