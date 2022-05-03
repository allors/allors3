namespace Scaffold
{
    public class Template
    {
        private readonly Scriban.Template template;

        public Template(Scriban.Template template) => this.template = template;

        public string Render(Model model) => this.template.Render(new { Model = model });
    }
}
