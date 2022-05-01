namespace Scaffold
{
    using AngleSharp.Dom;

    public class AssociationComponentModel : ComponentModel
    {
        public static readonly Dictionary<string, string> TypeByTag = new()
        {
            { "a-mat-autocomplete", "Allors.E2E.Angular.Material.Association.AllorsMaterialAutocompleteComponent" },
         };

        public override string Property { get; }

        public override string Type { get; }

        public override string Init { get; }

        public AssociationComponentModel(IElement element)
        {
            var tag = element.TagName.ToLowerInvariant();
            var fullType = TypeByTag[tag];

            var associationTypeAttribute = element.GetAttribute("[associationType]");
            var associationTypeParts = associationTypeAttribute.Split(".");
            var associationTypeObjectType = associationTypeParts[1];
            var associationTypeName = associationTypeParts[2];

            this.Type = fullType;
            this.Property = $"{associationTypeName}";
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
    }
}
