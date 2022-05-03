namespace Scaffold
{
    using AngleSharp.Dom;

    public class DefaultComponentModel : ComponentModel
    {
        public static readonly Dictionary<string, string> TypeByTag = new()
        {
            { "a-mat-barcode-entry", "Allors.E2E.Angular.Material.BarcodeEntryComponent" },
            { "a-mat-cancel", "Allors.E2E.Angular.Material.CancelComponent" },
            { "a-mat-dyn-view-detail-panel", "Allors.E2E.Angular.Material.Dynamic.AllorsMaterialDynamicViewDetailPanelComponent" },
            { "a-mat-dyn-edit-detail-panel", "Allors.E2E.Angular.Material.Dynamic.AllorsMaterialDynamicEditDetailPanelComponent" },
            { "a-mat-factory-fab", "Allors.E2E.Angular.Material.Factory.FactoryFabComponent" },
            { "a-mat-filter", "Allors.E2E.Angular.Material.Filter.FilterComponent" },
            { "a-mat-save", "Allors.E2E.Angular.Material.SaveComponent" },
        };

        public override string Type { get; }

        public override string Property { get; protected set; }

        public override string Init { get; }

        public override void ElevatePropertyName(ISet<string> properties)
        {
        }

        public DefaultComponentModel(IElement element)
        {
            var tag = element.TagName.ToLowerInvariant();
            var fullType = TypeByTag[tag];
            var type = fullType.Split('.').Last();

            this.Type = fullType;
            this.Property = type;
            this.Init = "new " + fullType + "(this);";
        }

        public class Builder : ComponentModelBuilder
        {
            public Builder(ComponentModelBuilder? next = null) : base(next)
            {
            }

            public override ComponentModel? Build(IElement element) =>
                TypeByTag.ContainsKey(element.TagName.ToLowerInvariant())
                    ? new DefaultComponentModel(element)
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
