namespace Scaffold
{
    using AngleSharp.Html.Dom;

    public class Generator
    {
        public ModelBuilder ModelBuilder { get; }

        public FileInfo[] Input { get; }

        public DirectoryInfo Output { get; }

        public string Namespace { get; }

        public Generator(ModelBuilder modelBuilder, string[] directories,
            string output, string @namespace)
        {
            this.ModelBuilder = modelBuilder;
            this.Namespace = @namespace;

            var fileByName = new Dictionary<string, FileInfo>();

            foreach (var directory in directories)
            {
                var files = Directory
                    .GetFiles(directory, "*.component.html", SearchOption.AllDirectories)
                    .Select(v => new FileInfo(v)).ToArray();

                foreach (var file in files)
                {
                    fileByName[file.Name] = file;
                }
            }

            this.Input = fileByName.Values.ToArray();

            this.Output = new DirectoryInfo(output);

            if (!this.Output.Exists)
            {
                this.Output.Create();
            }
        }

        public async Task Generate()
        {
            foreach (var input in this.Input)
            {
                var model = this.ModelBuilder.Build(input);

                if (model == null)
                {
                    continue;
                }

                await model.Init();

                var code = this.ModelBuilder.Template.Render(model);
                var output = Path.Combine(this.Output.FullName, $"{model.Class}.cs");
                await File.WriteAllTextAsync(output, code);

                Console.WriteLine($"Scaffold: {input.Name} => {output}");
            }
        }
    }
}
