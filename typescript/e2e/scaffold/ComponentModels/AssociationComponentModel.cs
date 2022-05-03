namespace Scaffold
{
    using AngleSharp.Dom;

    public class AssociationComponentModel : ComponentModel
    {
        public static readonly Dictionary<string, string> TypeByTag = new()
        {
            { "a-mat-autocomplete", "Allors.E2E.Angular.Material.Association.AllorsMaterialAutocompleteComponent" },
        };

        public static readonly Dictionary<string, string> SuffixByTag = new()
        {
            { "a-mat-autocomplete", "Autocomplete" },
        };

        public override string Property { get; protected set; }

        public override string Type { get; }

        public override string Init { get; }

        private string FullProperty { get; }

        public override void ElevatePropertyName(ISet<string> properties)
        {
            if (properties.Contains(this.Property))
            {
                this.Property = this.FullProperty;
            }
        }

        public AssociationComponentModel(IElement element)
        {
            var tag = element.TagName.ToLowerInvariant();
            var fullType = TypeByTag[tag];
            var suffix = SuffixByTag[tag];

            var associationTypeAttribute = element.GetAttribute("[associationType]");
            var associationTypeParts = associationTypeAttribute.Split(".");
            var associationTypeObjectType = associationTypeParts[1].Trim();
            var associationTypeName = associationTypeParts[2].Trim();

            this.Type = fullType;
            this.Property = $"{associationTypeName}{suffix}";
            this.FullProperty = $"{associationTypeObjectType}{associationTypeName}{suffix}";
            this.Init = $"new {fullType}(this, this.M.{associationTypeObjectType}.{associationTypeName});";
        }

        public class Builder : ComponentModelBuilder
        {
            public Builder(ComponentModelBuilder? next = null) : base(next)
            {
            }

            public override ComponentModel? Build(IElement element) =>
                TypeByTag.ContainsKey(element.TagName.ToLowerInvariant())
                    ? new AssociationComponentModel(element)
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
