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
        public string Namespace { get; } = "Allors.E2E.Test";

        [Argument(0)]
        [Required]
        public string[] Directories { get; }

        private async Task OnExecute()
        {
            try
            {
                var componentBuilder = new RoleComponentModel.Builder(new AssociationComponentModel.Builder(new ExtentComponentModel.Builder(new DefaultComponentModel.Builder())));
                var modelBuilder = new ContainerModel.Builder(componentBuilder, this.Namespace);
                var generator = new Generator(modelBuilder, this.Directories, this.Output, this.Namespace);
                await generator.Generate();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
