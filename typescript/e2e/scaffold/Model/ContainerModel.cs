namespace Scaffold
{
    public class ContainerModel : Model
    {
        public override string Html { get; }

        public override string Class { get; }

        public override string Namespace { get; }

        public override string Base => "Allors.E2E.ContainerComponent";

        public ContainerModel(ModelBuilder modelBuilder, FileInfo fileInfo) : base(modelBuilder)
        {
            this.Html = File.ReadAllText(fileInfo.FullName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);
            this.Class = fileNameWithoutExtension.ToPascalCase();
            this.Namespace = modelBuilder.Namespace;
        }

        public class Builder : ModelBuilder
        {
            public static readonly Scriban.Template Default = Scriban.Template.Parse(
                @"namespace {{ model.namespace }}
{
    public partial class {{ model.class }} : {{ model.base }}
    {

         public {{ model.class }}(global::Allors.E2E.IComponent container) : base(container) { }

{{ for component in model.components }}
        public {{ component.type }} {{ component.property }} => {{ component.init }}
{{ end }}
    }
}
");

            public Builder(ComponentModelBuilder componentModelBuilder, string ns,
                ModelBuilder? next = null) : base(next)
            {
                this.ComponentModelBuilder = componentModelBuilder;
                this.Template = new Template(Default);
                this.Namespace = ns;
            }

            public override Template Template { get; }

            public override string? Namespace { get; }

            public override ComponentModelBuilder ComponentModelBuilder { get; }

            public override Model? Build(FileInfo fileInfo) =>
                fileInfo.Name.ToLowerInvariant().Contains("-form.") ||
                fileInfo.Name.ToLowerInvariant().Contains("-dialog.")
                    ? new ContainerModel(this, fileInfo)
                    : base.Build(fileInfo);
        }
    }
}
