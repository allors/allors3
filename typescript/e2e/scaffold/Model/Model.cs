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
            var components = document.All
                .Where(m => m.TagName.StartsWith("a-", StringComparison.InvariantCultureIgnoreCase))
                .Select(this.ModelBuilder.ComponentModelBuilder.Build)
                .Where(v => v != null)
                .Select(e => e!)
                .ToArray();

            // Remove duplicates (*ngIf)
            this.Components = components.Aggregate(new List<ComponentModel>(), (acc, el) =>
            {
                if (!acc.Contains(el))
                {
                    acc.Add(el);
                }
                else
                {
                    Console.WriteLine(0);
                }

                return acc;
            }).ToArray();

            // Fully Qualified Names for Properties
            var properties = new HashSet<string>(this.Components
                .GroupBy(v => v.Property)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key));
            
            foreach (var component in this.Components)
            {
                component.ElevatePropertyName(properties);
            }
        }

    }
}
