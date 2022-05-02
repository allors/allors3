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

        public override string Property { get; }

        public override string Type { get; }

        public override string Init { get; }

        public ExtentComponentModel(IElement element)
        {
            var tag = element.TagName.ToLowerInvariant();
            var fullType = TypeByTag[tag];

            var @init = element.GetAttribute("[init]");
            var @select = element.GetAttribute("[select]");

            var initParts = @init?.Split(".");
            var selectParts = @select?.Split(".");

            var parts = initParts?.Length == 3 ? initParts : selectParts;

            var prefix = PrefixByTag[tag];
            if (parts?.Length == 3)
            {
                var propertyType = parts[2];
                this.Property = $"{prefix}{propertyType}";
            }
            else
            {
                var property = @init?.ToPascalCase() ?? @select?.ToPascalCase();
                this.Property = prefix + property;
            }


            this.Type = fullType;
            this.Init = $"new {fullType}(this, \"{@init}\", \"{@select}\");";
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
    }
}
