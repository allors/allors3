namespace Scaffold
{
    using Scriban;

    public class Generator
    {
        private readonly Template template;

        public Generator(Template template) => this.template = template;

        public string Render(Model model)
        {
            var result = this.template.Render(new { Model = model });
            return result;
        }
    }
}
