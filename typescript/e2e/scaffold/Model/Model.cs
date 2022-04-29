namespace Scaffold
{
    using AngleSharp;

    public abstract class Model
    {
        protected Model(ModelBuilder modelBuilder) => this.ModelBuilder = modelBuilder;

        public abstract string Html { get; }

        public abstract string Class { get; }

        public abstract string Namespace { get; }

        public abstract string Base { get; }

        public ComponentModel[] Components { get; protected set; }

        protected ModelBuilder ModelBuilder { get; }

        public virtual async Task Init()
        {
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(req => req.Content(this.Html));
            this.Components = document.All
                .Where(m => m.TagName.StartsWith("a-", StringComparison.InvariantCultureIgnoreCase))
                .Select(this.ModelBuilder.ComponentModelBuilder.Build)
                .Where(v => v != null)
                .Select(e => e!)
                .ToArray();
        }

    }
}
