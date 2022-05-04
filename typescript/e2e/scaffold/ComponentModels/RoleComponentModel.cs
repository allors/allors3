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


        public static readonly Dictionary<string, string> SuffixByTag = new()
        {
            { "a-mat-autocomplete", "Autocomplete" },
            { "a-mat-checkbox", "Checkbox" },
            { "a-mat-chips", "Chips" },
            { "a-mat-datepicker", "Datepicker" },
            { "a-mat-datetimepicker", "Datetimepicker" },
            { "a-mat-file", "File" },
            { "a-mat-files", "Files" },
            { "a-mat-input", "Input" },
            { "a-mat-localised-markdown", "LocalisedMarkdown" },
            { "a-mat-localised-text", "LocalisedText" },
            { "a-mat-markdown", "Markdown" },
            { "a-mat-radio-group", "RadioGroup" },
            { "a-mat-select", "Select" },
            { "a-mat-slider", "Slider" },
            { "a-mat-slidetoggle", "SlideToggle" },
            { "a-mat-static", "Static" },
            { "a-mat-textarea", "Textarea" },
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

        public RoleComponentModel(IElement element)
        {
            var tag = element.TagName.ToLowerInvariant();
            var fullType = TypeByTag[tag];
            var suffix = SuffixByTag[tag];

            var roleTypeAttribute = element.GetAttribute("[roleType]");
            var roleTypeParts = roleTypeAttribute.Split(".");
            var roleTypeObjectType = roleTypeParts[1].Trim();
            var roleTypeName = roleTypeParts[2].Trim();

            this.Type = fullType;
            this.Property = $"{roleTypeName}{suffix}";
            this.FullProperty = $"{roleTypeObjectType}{roleTypeName}{suffix}";
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
