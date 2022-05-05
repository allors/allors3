namespace Scaffold
{
    using AngleSharp.Dom;

    public class ExtentComponentModel : ComponentModel
    {
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

            var @include = element.GetAttribute("[include]")?.Trim();
            var @init = element.GetAttribute("[init]")?.Trim();
            var @select = element.GetAttribute("[select]")?.Trim();

            var includeParts = @include?.Split(".");
            var initParts = @init?.Split(".");
            var selectParts = @select?.Split(".");

            var parts = includeParts?.Length == 3 ? includeParts : initParts?.Length == 3 ? initParts : selectParts;

            var prefix = PrefixByTag[tag];
            if (parts?.Length == 3)
            {
                var objectType = parts[1];
                var propertyType = parts[2];
                this.Property = $"{prefix}{propertyType}";
                this.FullProperty = $"{prefix}{objectType}{propertyType}";
            }
            else
            {
                var property = @init?.ToPascalCase() ?? @select?.ToPascalCase();
                this.Property = prefix + property;
                this.FullProperty = this.Property;
            }


            this.Type = fullType;
            this.Init = $"new {fullType}(this, \"{@select}\", \"{@init}\", \"{@include}\");";
        }

        public class Builder : ComponentModelBuilder
        {
            public Builder(ComponentModelBuilder? next = null) : base(next)
            {
            }

            public override ComponentModel? Build(IElement element) =>
                TypeByTag.ContainsKey(element.TagName.ToLowerInvariant())
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
