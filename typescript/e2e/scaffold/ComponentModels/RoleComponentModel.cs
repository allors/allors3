namespace Scaffold
{
    using AngleSharp.Dom;

    public class RoleComponentModel : ComponentModel
    {
        public static readonly Dictionary<string, string> TypeByTag = new()
        {
            { "a-mat-autocomplete", "Allors.E2E.Angular.Material.Role.AllorsMaterialAutocompleteComponent" },
            { "a-mat-checkbox", "Allors.E2E.Angular.Material.Role.AllorsMaterialCheckboxComponent" },
            { "a-mat-chips", "Allors.E2E.Angular.Material.Role.AllorsMaterialChipsComponent" },
            { "a-mat-datepicker", "Allors.E2E.Angular.Material.Role.AllorsMaterialDatepickerComponent" },
            { "a-mat-datetimepicker", "Allors.E2E.Angular.Material.Role.AllorsMaterialDatetimepickerComponent" },
            { "a-mat-file", "Allors.E2E.Angular.Material.Role.AllorsMaterialFileComponent" },
            { "a-mat-files", "Allors.E2E.Angular.Material.Role.AllorsMaterialFilesComponent" },
            { "a-mat-input", "Allors.E2E.Angular.Material.Role.AllorsMaterialInputComponent" },
            { "a-mat-localised-markdown", "Allors.E2E.Angular.Material.Role.AllorsMaterialLocalisedMarkdownComponent" },
            { "a-mat-localised-text", "Allors.E2E.Angular.Material.Role.AllorsMaterialLocalisedTextComponent" },
            { "a-mat-markdown", "Allors.E2E.Angular.Material.Role.AllorsMaterialMarkdownComponent" },
            { "a-mat-radio-group", "Allors.E2E.Angular.Material.Role.AllorsMaterialRadioGroupComponent" },
            { "a-mat-select", "Allors.E2E.Angular.Material.Role.AllorsMaterialSelectComponent" },
            { "a-mat-slider", "Allors.E2E.Angular.Material.Role.AllorsMaterialSliderComponent" },
            { "a-mat-slidetoggle", "Allors.E2E.Angular.Material.Role.AllorsMaterialSlideToggleComponent" },
            { "a-mat-static", "Allors.E2E.Angular.Material.Role.AllorsMaterialStaticComponent" },
            { "a-mat-textarea", "Allors.E2E.Angular.Material.Role.AllorsMaterialTextareaComponent" },
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
