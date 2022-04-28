namespace Scaffold
{
    public class Template
    {
        public static readonly Scriban.Template Default = Scriban.Template.Parse(
            @"namespace {{ model.ns }}
{
    public class {{ model.cls }} : global::Allors.E2E.ContainerComponent
    {

         public {{ model.cls }}(global::Allors.E2E.IComponent container) : base(container) { }

{{ for component in model.components }}
        public {{ component.type }} {{ component.property }} => {{ component.init }}
{{ end }}
    }
}
");

        private readonly Scriban.Template template;

        public Template(Scriban.Template template) => this.template = template;

        public string Render(Model model) => this.template.Render(new { Model = model });
    }
}
