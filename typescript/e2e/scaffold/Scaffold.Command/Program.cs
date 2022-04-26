namespace Scaffold // Note: actual namespace depends on the project name.
{
    using System.ComponentModel.DataAnnotations;
    using McMaster.Extensions.CommandLineUtils;
    using Scriban;

    internal class Program
    {
        public static int Main(string[] args)
            => CommandLineApplication.Execute<Program>(args);

        [Option(Description = "Input file")]
        [Required]
        public string Input { get; }

        [Option(Description = "Output file")]
        [Required]
        public string Output { get; }

        [Option(Description = "Class name")]
        public string Class { get; }

        [Option(Description = "Namespace")] public string Namespace { get; } = "Allors.E2E.Angular.Components";

        private async Task OnExecute()
        {
            try
            {
                var input = await File.ReadAllTextAsync(this.Input);
                var cls = this.Class ?? Path.GetFileNameWithoutExtension(this.Output);
                var model = new Model(cls, this.Namespace);
                await model.Init(input);

                var template = Template.Parse(
@"namespace {{ model.ns }}
{
    public class {{ model.cls }} {
{{ for component in model.components }}
        public Component {{ component.name }} { get; }
{{ end }}
    }
}
");
                var generator = new Generator(template);
                var code = generator.Render(model);

                File.WriteAllText(this.Output, code);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
