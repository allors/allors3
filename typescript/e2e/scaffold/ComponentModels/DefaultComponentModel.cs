namespace Scaffold
{
    using AngleSharp.Dom;

    public class DefaultComponentModel : ComponentModel
    {
        public static readonly Dictionary<string, string> TypeByTag = new()
        {
            { "a-mat-barcode-entry", "Allors.E2E.Angular.Material.BarcodeComponent" },
        };

        public override string Type { get; }

        public override string Property { get; }

        public override string Init { get; }

        public DefaultComponentModel(IElement element)
        {
            var tag = element.TagName.ToLowerInvariant();
            var fullType = TypeByTag[tag];
            var type = fullType.Split('.').Last();

            this.Type = fullType;
            this.Property = type;
            this.Init = "new " + fullType + "(this, );";
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
