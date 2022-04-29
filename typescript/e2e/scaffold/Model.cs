namespace Scaffold
{
    using AngleSharp;

    public class Model
    {
        public ComponentModelBuilder ComponentModelBuilder { get; }

        public string Cls { get; private set; }

        public string Ns { get; private set; }

        public ComponentModel[] Components { get; private set; }

        public Model(ComponentModelBuilder componentModelBuilder, string cls, string ns)
        {
            this.ComponentModelBuilder = componentModelBuilder;
            this.Cls = cls;
            this.Ns = ns;
        }

        public async Task Init(string input)
        {
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(req => req.Content(input));
            this.Components = document.All
                .Where(m => m.TagName.StartsWith("a-", StringComparison.InvariantCultureIgnoreCase))
                .Select(this.ComponentModelBuilder.Build)
                .Where(v => v != null)
                .Select(e => e!)
                .ToArray();
        }

    }
}
