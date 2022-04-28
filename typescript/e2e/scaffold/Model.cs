namespace Scaffold
{
    using AngleSharp;
    using AngleSharp.Dom;

    public class Model
    {
        public string Cls { get; private set; }

        public string Ns { get; private set; }

        public ComponentModel[] Components { get; private set; }

        public Model(string cls, string ns)
        {
            this.Cls = cls;
            this.Ns = ns;
        }

        public async Task Init(string input, Func<string, Func<IElement, ComponentModel>?> factory)
        {
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(req => req.Content(input));
            this.Components = document.All
                .Where(m => m.TagName.StartsWith("a-", StringComparison.InvariantCultureIgnoreCase))
                .Select(v => factory(v.TagName.ToLowerInvariant())?.Invoke(v))
                .Where(v => v != null)
                .Select(e => e!)
                .ToArray();
        }

    }
}
