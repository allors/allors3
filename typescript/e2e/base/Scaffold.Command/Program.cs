namespace Scaffold // Note: actual namespace depends on the project name.
{
    using System.ComponentModel.DataAnnotations;
    using McMaster.Extensions.CommandLineUtils;

    internal class Program
    {
        public static int Main(string[] args)
            => CommandLineApplication.Execute<Program>(args);


        [Option(Description = "Output directory")]
        [Required]
        public string Output { get; }

        [Option(Description = "Namespace")]
        public string Namespace { get; } = "Allors.E2E.Angular.Material.Generated";

        [Argument(0)]
        [Required]
        public string[] Directories { get; }

        private async Task OnExecute()
        {
            try
            {
                var builder = new RoleComponentModel.Builder(new DefaultComponentModel.Builder());
                var template = new Template(Template.Default);
                var generator = new Generator(template, builder, this.Directories, this.Output, this.Namespace);
                await generator.Generate();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
