namespace Scaffold
{
    using System.Globalization;

    public class Generator
    {
        public Template Template { get; }

        public FileInfo[] Input { get; }

        public DirectoryInfo Output { get; }

        public string Namespace { get; }

        public Generator(Template template, string[] directories, string output, string @namespace)
        {
            this.Template = template;
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
                var html = await File.ReadAllTextAsync(input.FullName);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(input.Name);
                var cls = fileNameWithoutExtension.ToPascalCase();

                var model = new Model(cls, this.Namespace);



                await model.Init(html, (tag) =>
                {
                    if (RoleComponentModel.TypeByTag.ContainsKey(tag))
                    {
                        return (element) => new RoleComponentModel(element);
                    }

                    if (DefaultComponentModel.TypeByTag.ContainsKey(tag))
                    {
                        return (element) => new DefaultComponentModel(element);
                    }

                    return null;
                });

                if (model.Components.Length <= 0)
                {
                    continue;
                }

                var code = this.Template.Render(model);
                var output = Path.Combine(this.Output.FullName, $"{cls}.cs");
                await File.WriteAllTextAsync(output, code);
            }
        }

     
    }
}
